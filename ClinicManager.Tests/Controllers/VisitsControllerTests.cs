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
                    LastName = "WiÅ›niewski",
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

