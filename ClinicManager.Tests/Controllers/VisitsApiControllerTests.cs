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
                    DoctorFullName = "Adam WiÅ›niewski",
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

    [Test]
    public async Task GetActiveVisits_WhenServiceReturnsEmptyList_ReturnsOkWithEmptyList()
    {
        var controller = new VisitsApiController(
            new ActiveVisitsService(),
            NullLogger<VisitsApiController>.Instance);

        var result = await controller.GetActiveVisits(CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.Empty);
    }

    [Test]
    public async Task GetActiveVisits_PassesCancellationTokenToService()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var visitService = new ActiveVisitsService();
        var controller = new VisitsApiController(
            visitService,
            NullLogger<VisitsApiController>.Instance);

        await controller.GetActiveVisits(cancellationTokenSource.Token);

        Assert.That(visitService.LastCancellationToken, Is.EqualTo(cancellationTokenSource.Token));
    }

    private sealed class ActiveVisitsService : IVisitService
    {
        public IReadOnlyList<ActiveVisitDto> ActiveVisits { get; set; } = Array.Empty<ActiveVisitDto>();
        public bool GetActiveVisitsWasCalled { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

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
            LastCancellationToken = cancellationToken;

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

