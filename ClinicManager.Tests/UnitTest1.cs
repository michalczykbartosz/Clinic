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

public class PatientRecordDtoTests
{
    [Test]
    public void UpdatePatientRecordDto_WhenRequiredFieldsAreInvalid_FailsValidation()
    {
        var dto = new UpdatePatientRecordDto
        {
            PESEL = "abc",
            InsuranceNumber = "   "
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            dto,
            new ValidationContext(dto),
            validationResults,
            validateAllProperties: true);

        var invalidMembers = validationResults.SelectMany(result => result.MemberNames);

        Assert.That(isValid, Is.False);
        Assert.That(invalidMembers, Does.Contain(nameof(UpdatePatientRecordDto.PESEL)));
        Assert.That(invalidMembers, Does.Contain(nameof(UpdatePatientRecordDto.InsuranceNumber)));
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

        await using var dbContext = await CreateVisitDbContextAsync();
        var userManager = CreateVisitUserManager();

        var controller = new VisitsController(
            new VisitFormVisitService(),
            userManager,
            dbContext,
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
        await using var dbContext = await CreateVisitDbContextAsync();
        var userManager = CreateVisitUserManager();
        var visitService = new VisitFormVisitService();
        var controller = new VisitsController(
            visitService,
            userManager,
            dbContext,
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
        await using var dbContext = await CreateVisitDbContextAsync();
        var userManager = CreateVisitUserManager();
        var visitService = new VisitFormVisitService();
        var controller = new VisitsController(
            visitService,
            userManager,
            dbContext,
            NullLogger<VisitsController>.Instance)
        {
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                new TestTempDataProvider())
        };

        var result = await controller.UpdateStatus(
            10,
            new UpdateVisitStatusDto { VisitStatus = VisitState.Finished },
            null,
            null,
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
        await using var dbContext = await CreateVisitDbContextAsync();
        var userManager = CreateVisitUserManager();
        var visitService = new VisitFormVisitService { UpdateStatusResult = false };
        var controller = new VisitsController(
            visitService,
            userManager,
            dbContext,
            NullLogger<VisitsController>.Instance);

        var result = await controller.UpdateStatus(
            999,
            new UpdateVisitStatusDto { VisitStatus = VisitState.Canceled },
            null,
            null,
            CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    private static async Task<ClinicDbContext> CreateVisitDbContextAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new ClinicDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        return dbContext;
    }

    private static TestUserManager CreateVisitUserManager()
    {
        var userManager = new TestUserManager();
        userManager.AddUserToRole("Pacjent", "90051401234");
        userManager.AddUserToRole("Lekarz", "75081911223");

        return userManager;
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

        public Task<IReadOnlyList<VisitListItemDto>> GetListAsync(
            string? query = null,
            VisitState? status = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<VisitListItemDto>>(Array.Empty<VisitListItemDto>());
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

        public Task<bool> UpdatePaymentAsync(int visitId, bool isPaid, CancellationToken cancellationToken = default)
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

internal sealed class TestUserManager : UserManager<IdentityUser>
{
    private readonly Dictionary<string, List<IdentityUser>> _usersByRole = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IList<Claim>> _claimsByUserId = new();

    public TestUserManager()
        : base(
            new TestUserStore(),
            null!,
            new PasswordHasher<IdentityUser>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<IdentityUser>>.Instance)
    {
    }

    public void AddUserToRole(string roleName, string pesel)
    {
        var user = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"{roleName}-{pesel}"
        };

        if (!_usersByRole.TryGetValue(roleName, out var users))
        {
            users = [];
            _usersByRole[roleName] = users;
        }

        users.Add(user);
        _claimsByUserId[user.Id] = [new Claim("PatientPesel", pesel)];
    }

    public override Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName)
    {
        var users = _usersByRole.TryGetValue(roleName, out var roleUsers)
            ? roleUsers.ToList()
            : [];

        return Task.FromResult<IList<IdentityUser>>(users);
    }

    public override Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
    {
        var claims = _claimsByUserId.TryGetValue(user.Id, out var userClaims)
            ? userClaims.ToList()
            : [];

        return Task.FromResult<IList<Claim>>(claims);
    }
}

internal sealed class TestUserStore : IUserStore<IdentityUser>
{
    public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public void Dispose()
    {
    }

    public Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IdentityUser?>(null);
    }

    public Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return Task.FromResult<IdentityUser?>(null);
    }

    public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(IdentityUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }
}

public class VisitsApiControllerTests
{
    [Test]
    public async Task GetActiveVisits_ReturnsActiveVisitsFromService()
    {
        var visitService = new ActiveVisitsService
        {
            ActiveVisits =
            [
                new ActiveVisitDto
                {
                    VisitId = 12,
                    VisitStatus = VisitState.InProgress,
                    VisitDateTime = new DateTime(2026, 6, 16, 11, 30, 0),
                    PatientId = 3,
                    PatientFullName = "Anna Kowalska",
                    PatientPESEL = "91020312345",
                    DoctorId = 4,
                    DoctorFullName = "Adam Wiśniewski",
                    DoctorSpecialization = "Kardiolog"
                }
            ]
        };
        var controller = new VisitsApiController(
            visitService,
            NullLogger<VisitsApiController>.Instance);

        var result = await controller.GetActiveVisits(CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var model = okResult!.Value as IReadOnlyList<ActiveVisitDto>;
        Assert.That(model, Is.Not.Null);
        Assert.That(model!, Has.Count.EqualTo(1));
        Assert.That(model![0].PatientFullName, Is.EqualTo("Anna Kowalska"));
        Assert.That(visitService.GetActiveVisitsWasCalled, Is.True);
    }

    private sealed class ActiveVisitsService : IVisitService
    {
        public IReadOnlyList<ActiveVisitDto> ActiveVisits { get; set; } = Array.Empty<ActiveVisitDto>();
        public bool GetActiveVisitsWasCalled { get; private set; }

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

        public Task<IReadOnlyList<PatientVisitDto>> GetByPatientIdAsync(
            int patientId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ActiveVisitDto>> GetActiveVisitsAsync(
            CancellationToken cancellationToken = default)
        {
            GetActiveVisitsWasCalled = true;

            return Task.FromResult(ActiveVisits);
        }

        public Task<VisitDto> CreateAsync(CreateVisitDto dto, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateStatusAsync(
            int visitId,
            VisitState visitStatus,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdatePaymentAsync(
            int visitId,
            bool isPaid,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

public class VisitServiceActiveVisitsTests
{
    [Test]
    public async Task GetActiveVisitsAsync_ReturnsOnlyActiveVisitsWithPatientAndDoctorData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new ClinicDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Visits.Add(new Visit
        {
            VisitId = 3,
            VisitStatus = VisitState.Finished,
            PatientId = 1,
            DoctorId = 1,
            VisitDateTime = new DateTime(2026, 6, 17, 9, 0, 0)
        });
        await dbContext.SaveChangesAsync();

        var service = new VisitService(
            dbContext,
            new VisitMapper(),
            NullLogger<VisitService>.Instance);

        var visits = await service.GetActiveVisitsAsync(CancellationToken.None);

        Assert.That(visits, Has.Count.EqualTo(2));
        Assert.That(visits.Select(visit => visit.VisitStatus), Is.All.Matches<VisitState>(
            status => status is VisitState.Planned or VisitState.InProgress));
        Assert.That(visits[0].PatientFullName, Is.EqualTo("Jan Nowak"));
        Assert.That(visits[0].PatientPESEL, Is.EqualTo("90051401234"));
        Assert.That(visits[0].DoctorFullName, Is.EqualTo("Ewa Kowalczyk"));
        Assert.That(visits[0].DoctorSpecialization, Is.EqualTo("Neurolog"));
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
