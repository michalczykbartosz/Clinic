using System.ComponentModel.DataAnnotations;
using ClinicManager.Controllers;
using ClinicManager.Controllers.Api;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace ClinicManager.Tests;

public class PatientsControllerTests
{
    [Test]
    public async Task Details_WhenPatientExists_ReturnsPatientWithVisits()
    {
        var patientService = new StubPatientService
        {
            Patient = new PatientDto
            {
                PatientId = 1,
                FirstName = "Jan",
                LastName = "Nowak",
                PESEL = "90051401234",
                InsuranceNumber = "NFZ-998877",
                BirthDate = new DateOnly(1990, 5, 14)
            }
        };

        var visitService = new StubVisitService
        {
            Visits =
            [
                new PatientVisitDto
                {
                    VisitId = 7,
                    VisitStatus = VisitState.Planned,
                    VisitDateTime = new DateTime(2026, 6, 15, 14, 0, 0),
                    DoctorId = 3,
                    DoctorFullName = "Adam WiÅ›niewski",
                    DoctorSpecialization = "Kardiolog"
                }
            ]
        };

        var controller = new PatientsController(
            patientService,
            visitService,
            new StubPatientDocumentService
            {
                Documents =
                [
                    new PatientDocumentDto
                    {
                        PatientDocumentId = 4,
                        PatientId = 1,
                        OriginalFileName = "skierowanie.pdf",
                        ContentType = "application/pdf",
                        FileSize = 1024,
                        UploadedAt = new DateTime(2026, 6, 16, 9, 0, 0)
                    }
                ]
            },
            NullLogger<PatientsController>.Instance);

        var result = await controller.Details(1, CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);

        var model = viewResult!.Model as PatientDetailsViewModel;
        Assert.That(model, Is.Not.Null);
        Assert.That(model!.Patient.PatientId, Is.EqualTo(1));
        Assert.That(model.Visits, Has.Count.EqualTo(1));
        Assert.That(model.Visits[0].DoctorFullName, Is.EqualTo("Adam WiÅ›niewski"));
        Assert.That(model.Documents, Has.Count.EqualTo(1));
        Assert.That(model.Documents[0].OriginalFileName, Is.EqualTo("skierowanie.pdf"));
    }

    [Test]
    public async Task Details_WhenPatientDoesNotExist_ReturnsNotFound()
    {
        var controller = new PatientsController(
            new StubPatientService(),
            new StubVisitService(),
            new StubPatientDocumentService(),
            NullLogger<PatientsController>.Instance);

        var result = await controller.Details(999, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Record_Get_WhenPatientExists_ReturnsRecordForm()
    {
        var controller = new PatientsController(
            new StubPatientService
            {
                Patient = new PatientDto
                {
                    PatientId = 2,
                    FirstName = "Anna",
                    LastName = "Kowalska",
                    PESEL = "91020312345",
                    InsuranceNumber = "NFZ-123"
                }
            },
            new StubVisitService(),
            new StubPatientDocumentService(),
            NullLogger<PatientsController>.Instance);

        var result = await controller.Record(2, CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);

        var model = viewResult!.Model as UpdatePatientRecordDto;
        Assert.That(model, Is.Not.Null);
        Assert.That(model!.PESEL, Is.EqualTo("91020312345"));
        Assert.That(model.InsuranceNumber, Is.EqualTo("NFZ-123"));
    }

    [Test]
    public async Task Record_Post_WhenModelIsValid_UpdatesRecordAndRedirectsToDetails()
    {
        var patientService = new StubPatientService();
        var controller = new PatientsController(
            patientService,
            new StubVisitService(),
            new StubPatientDocumentService(),
            NullLogger<PatientsController>.Instance)
        {
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                new TestTempDataProvider())
        };

        var dto = new UpdatePatientRecordDto
        {
            PESEL = "91020312345",
            InsuranceNumber = "NFZ-123"
        };

        var result = await controller.Record(2, dto, CancellationToken.None);

        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Details"));
        Assert.That(redirectResult.RouteValues!["id"], Is.EqualTo(2));
        Assert.That(patientService.UpdatedRecordPatientId, Is.EqualTo(2));
        Assert.That(patientService.UpdatedRecord, Is.SameAs(dto));
    }

    private sealed class StubPatientService : IPatientService
    {
        public PatientDto? Patient { get; set; }
        public int? UpdatedRecordPatientId { get; private set; }
        public UpdatePatientRecordDto? UpdatedRecord { get; private set; }
        public bool UpdateRecordResult { get; set; } = true;

        public Task<IReadOnlyList<PatientDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<PatientDto>> SearchAsync(string? query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PatientDto?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Patient);
        }

        public Task<PatientDto?> GetByPeselAsync(string pesel, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PatientDto> CreateAsync(UpsertPatientDto dto, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(int patientId, UpsertPatientDto dto, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateRecordAsync(
            int patientId,
            UpdatePatientRecordDto dto,
            CancellationToken cancellationToken = default)
        {
            UpdatedRecordPatientId = patientId;
            UpdatedRecord = dto;

            return Task.FromResult(UpdateRecordResult);
        }

        public Task<bool> DeleteAsync(int patientId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class StubVisitService : IVisitService
    {
        public IReadOnlyList<PatientVisitDto> Visits { get; set; } = Array.Empty<PatientVisitDto>();

        public Task<IReadOnlyList<VisitDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<VisitListItemDto>> GetListAsync(
            string? query = null,
            VisitState? status = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<VisitListItemDto>> GetListForPatientPeselAsync(
            string pesel,
            VisitState? status = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<VisitListItemDto>> GetDoctorScheduleAsync(
            int doctorId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<VisitDto?> GetByIdAsync(int visitId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<PatientVisitDto>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Visits);
        }

        public Task<IReadOnlyList<ActiveVisitDto>> GetActiveVisitsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<VisitDto> CreateAsync(CreateVisitDto dto, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateStatusAsync(int visitId, VisitState visitStatus, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdatePaymentAsync(int visitId, bool isPaid, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class StubPatientDocumentService : IPatientDocumentService
    {
        public IReadOnlyList<PatientDocumentDto> Documents { get; set; } = Array.Empty<PatientDocumentDto>();

        public Task<IReadOnlyList<PatientDocumentDto>> GetByPatientIdAsync(
            int patientId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Documents);
        }

        public Task<PatientDocumentDto?> UploadAsync(
            UploadPatientDocumentDto dto,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PatientDocumentFileDto?> GetFileAsync(
            int documentId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int?> DeleteAsync(int documentId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }


    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            return new Dictionary<string, object>();
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}

