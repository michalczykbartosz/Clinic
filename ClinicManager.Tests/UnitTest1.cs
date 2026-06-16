using System.ComponentModel.DataAnnotations;
using ClinicManager.Controllers;
using ClinicManager.DTOs;
using ClinicManager.Models;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        Assert.That(model.Visits[0].DoctorFullName, Is.EqualTo("Adam Wiśniewski"));
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

        public Task<IReadOnlyList<VisitListItemDto>> GetListAsync(CancellationToken cancellationToken = default)
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
}

public class PatientDocumentDtoTests
{
    [Test]
    public void UploadPatientDocumentDto_WhenFileIsMissing_FailsValidation()
    {
        var dto = new UploadPatientDocumentDto
        {
            PatientId = 1,
            File = null
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            dto,
            new ValidationContext(dto),
            validationResults,
            validateAllProperties: true);

        var invalidMembers = validationResults.SelectMany(result => result.MemberNames);

        Assert.That(isValid, Is.False);
        Assert.That(invalidMembers, Does.Contain(nameof(UploadPatientDocumentDto.File)));
    }
}

public class VisitsControllerTests
{
    [Test]
    public async Task Create_Get_ReturnsFormWithPatientsAndDoctors()
    {
        var patientService = new VisitFormPatientService
        {
            Patients =
            [
                new PatientDto
                {
                    PatientId = 1,
                    FirstName = "Jan",
                    LastName = "Nowak",
                    PESEL = "90051401234"
                }
            ]
        };

        var doctorService = new VisitFormDoctorService
        {
            Doctors =
            [
                new DoctorDto
                {
                    DoctorId = 2,
                    FirstName = "Adam",
                    LastName = "Wiśniewski",
                    Specialization = "Kardiolog"
                }
            ]
        };

        var controller = new VisitsController(
            new VisitFormVisitService(),
            patientService,
            doctorService,
            NullLogger<VisitsController>.Instance);

        var result = await controller.Create(CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);

        var model = viewResult!.Model as CreateVisitViewModel;
        Assert.That(model, Is.Not.Null);
        Assert.That(model!.Patients, Has.Count.EqualTo(1));
        Assert.That(model.Doctors, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Create_Post_WhenModelIsValid_CreatesVisitAndRedirectsToIndex()
    {
        var visitService = new VisitFormVisitService();
        var controller = new VisitsController(
            visitService,
            new VisitFormPatientService(),
            new VisitFormDoctorService(),
            NullLogger<VisitsController>.Instance)
        {
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                new TestTempDataProvider())
        };

        var model = new CreateVisitViewModel
        {
            Visit = new CreateVisitDto
            {
                PatientId = 1,
                DoctorId = 2,
                VisitDateTime = DateTime.Now.AddDays(1)
            }
        };

        var result = await controller.Create(model, CancellationToken.None);

        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Index"));
        Assert.That(visitService.CreatedVisit, Is.Not.Null);
        Assert.That(visitService.CreatedVisit!.PatientId, Is.EqualTo(1));
        Assert.That(visitService.CreatedVisit.DoctorId, Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateStatus_WhenVisitExists_UpdatesStatusAndRedirectsToIndex()
    {
        var visitService = new VisitFormVisitService();
        var controller = new VisitsController(
            visitService,
            new VisitFormPatientService(),
            new VisitFormDoctorService(),
            NullLogger<VisitsController>.Instance)
        {
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                new TestTempDataProvider())
        };

        var result = await controller.UpdateStatus(
            10,
            new UpdateVisitStatusDto { VisitStatus = VisitState.Finished },
            CancellationToken.None);

        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Index"));
        Assert.That(visitService.UpdatedVisitId, Is.EqualTo(10));
        Assert.That(visitService.UpdatedStatus, Is.EqualTo(VisitState.Finished));
    }

    [Test]
    public async Task UpdateStatus_WhenVisitDoesNotExist_ReturnsNotFound()
    {
        var visitService = new VisitFormVisitService { UpdateStatusResult = false };
        var controller = new VisitsController(
            visitService,
            new VisitFormPatientService(),
            new VisitFormDoctorService(),
            NullLogger<VisitsController>.Instance);

        var result = await controller.UpdateStatus(
            999,
            new UpdateVisitStatusDto { VisitStatus = VisitState.Canceled },
            CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    private sealed class VisitFormPatientService : IPatientService
    {
        public IReadOnlyList<PatientDto> Patients { get; set; } = Array.Empty<PatientDto>();

        public Task<IReadOnlyList<PatientDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Patients);
        }

        public Task<IReadOnlyList<PatientDto>> SearchAsync(string? query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PatientDto?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default)
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

        public Task<bool> DeleteAsync(int patientId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class VisitFormDoctorService : IDoctorService
    {
        public IReadOnlyList<DoctorDto> Doctors { get; set; } = Array.Empty<DoctorDto>();

        public Task<IReadOnlyList<DoctorDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Doctors);
        }

        public Task<DoctorDto?> GetByIdAsync(int doctorId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class VisitFormVisitService : IVisitService
    {
        public CreateVisitDto? CreatedVisit { get; private set; }
        public int? UpdatedVisitId { get; private set; }
        public VisitState? UpdatedStatus { get; private set; }
        public bool UpdateStatusResult { get; set; } = true;

        public Task<IReadOnlyList<VisitDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<VisitListItemDto>> GetListAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<VisitListItemDto>>(Array.Empty<VisitListItemDto>());
        }

        public Task<VisitDto?> GetByIdAsync(int visitId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<PatientVisitDto>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ActiveVisitDto>> GetActiveVisitsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<VisitDto> CreateAsync(CreateVisitDto dto, CancellationToken cancellationToken = default)
        {
            CreatedVisit = dto;

            return Task.FromResult(new VisitDto
            {
                VisitId = 10,
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                VisitDateTime = dto.VisitDateTime,
                VisitStatus = VisitState.Planned
            });
        }

        public Task<bool> UpdateStatusAsync(int visitId, VisitState visitStatus, CancellationToken cancellationToken = default)
        {
            UpdatedVisitId = visitId;
            UpdatedStatus = visitStatus;

            return Task.FromResult(UpdateStatusResult);
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

public class VisitMedicationsControllerTests
{
    [Test]
    public void CreateVisitMedicationDto_WhenRequiredFieldsAreInvalid_FailsValidation()
    {
        var dto = new CreateVisitMedicationDto
        {
            VisitId = 5,
            MedicationId = 0,
            Dosage = "   ",
            Quantity = 0
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            dto,
            new ValidationContext(dto),
            validationResults,
            validateAllProperties: true);

        var invalidMembers = validationResults.SelectMany(result => result.MemberNames);

        Assert.That(isValid, Is.False);
        Assert.That(invalidMembers, Does.Contain(nameof(CreateVisitMedicationDto.MedicationId)));
        Assert.That(invalidMembers, Does.Contain(nameof(CreateVisitMedicationDto.Dosage)));
        Assert.That(invalidMembers, Does.Contain(nameof(CreateVisitMedicationDto.Quantity)));
    }

    [Test]
    public async Task Index_WhenVisitExists_ReturnsMedicationList()
    {
        var service = new StubVisitMedicationService
        {
            Model = new VisitMedicationsViewModel
            {
                VisitId = 5,
                PatientId = 1,
                PatientFullName = "Jan Nowak",
                DoctorFullName = "Adam Wiśniewski",
                VisitDateTime = new DateTime(2026, 6, 16, 10, 0, 0),
                Medications =
                [
                    new VisitMedicationDto
                    {
                        PrescriptionItemId = 3,
                        MedicationId = 2,
                        MedicationName = "Ibuprom Max",
                        Manufacturer = "US Pharmacia",
                        Dose = "400mg",
                        Dosage = "1 tabletka rano",
                        Quantity = 1
                    }
                ]
            }
        };

        var controller = new VisitMedicationsController(
            service,
            NullLogger<VisitMedicationsController>.Instance);

        var result = await controller.Index(5, CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);

        var model = viewResult!.Model as VisitMedicationsViewModel;
        Assert.That(model, Is.Not.Null);
        Assert.That(model!.Medications, Has.Count.EqualTo(1));
        Assert.That(model.Medications[0].Dosage, Is.EqualTo("1 tabletka rano"));
    }

    [Test]
    public async Task Create_Post_WhenMedicationIsValid_AddsMedicationAndRedirectsToIndex()
    {
        var service = new StubVisitMedicationService { AddedVisitId = 5 };
        var controller = new VisitMedicationsController(
            service,
            NullLogger<VisitMedicationsController>.Instance)
        {
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                new TestTempDataProvider())
        };

        var model = new CreateVisitMedicationViewModel
        {
            Medication = new CreateVisitMedicationDto
            {
                VisitId = 5,
                MedicationId = 2,
                Dosage = "1 tabletka rano",
                Quantity = 2
            }
        };

        var result = await controller.Create(model, CancellationToken.None);

        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Index"));
        Assert.That(redirectResult.RouteValues!["visitId"], Is.EqualTo(5));
        Assert.That(service.AddedMedication, Is.Not.Null);
        Assert.That(service.AddedMedication!.MedicationId, Is.EqualTo(2));
        Assert.That(service.AddedMedication.Quantity, Is.EqualTo(2));
    }

    private sealed class StubVisitMedicationService : IVisitMedicationService
    {
        public VisitMedicationsViewModel? Model { get; set; }
        public int? AddedVisitId { get; set; }
        public CreateVisitMedicationDto? AddedMedication { get; private set; }

        public Task<VisitMedicationsViewModel?> GetForVisitAsync(int visitId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Model);
        }

        public Task<CreateVisitMedicationViewModel?> BuildCreateModelAsync(int visitId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<CreateVisitMedicationViewModel?>(new CreateVisitMedicationViewModel
            {
                Medication = new CreateVisitMedicationDto { VisitId = visitId }
            });
        }

        public Task<int?> AddMedicationAsync(CreateVisitMedicationDto dto, CancellationToken cancellationToken = default)
        {
            AddedMedication = dto;
            return Task.FromResult(AddedVisitId);
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
