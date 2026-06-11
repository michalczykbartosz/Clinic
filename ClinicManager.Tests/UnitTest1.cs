using ClinicManager.Controllers;
using ClinicManager.DTOs;
using ClinicManager.Models;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class PatientDetailsTests
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
                    DoctorFullName = "Adam Wiśniewski",
                    DoctorSpecialization = "Kardiolog"
                }
            ]
        };

        var controller = new PatientsController(
            patientService,
            visitService,
            NullLogger<PatientsController>.Instance);

        var result = await controller.Details(1, CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);

        var model = viewResult!.Model as PatientDetailsViewModel;
        Assert.That(model, Is.Not.Null);
        Assert.That(model!.Patient.PatientId, Is.EqualTo(1));
        Assert.That(model.Visits, Has.Count.EqualTo(1));
        Assert.That(model.Visits[0].DoctorFullName, Is.EqualTo("Adam Wiśniewski"));
    }

    [Test]
    public async Task Details_WhenPatientDoesNotExist_ReturnsNotFound()
    {
        var controller = new PatientsController(
            new StubPatientService(),
            new StubVisitService(),
            NullLogger<PatientsController>.Instance);

        var result = await controller.Details(999, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    private sealed class StubPatientService : IPatientService
    {
        public PatientDto? Patient { get; set; }

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

        public Task<PatientDto> CreateAsync(UpsertPatientDto dto, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(int patientId, UpsertPatientDto dto, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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
    }
}
