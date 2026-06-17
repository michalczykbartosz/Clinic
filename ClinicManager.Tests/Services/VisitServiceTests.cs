using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using ClinicManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class VisitServiceTests
{
    [Test]
    public async Task GetActiveVisitsAsync_ReturnsOnlyPlannedAndInProgressVisits()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        await SeedVisitDataAsync(dbContext);
        var service = CreateService(dbContext);

        var visits = await service.GetActiveVisitsAsync(CancellationToken.None);

        Assert.That(visits, Has.Count.EqualTo(2));
        Assert.That(visits.Select(visit => visit.VisitStatus), Is.All.Matches<VisitState>(
            status => status is VisitState.Planned or VisitState.InProgress));
    }

    [Test]
    public async Task GetListAsync_WhenQueryMatchesPesel_ReturnsMatchingVisit()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        await SeedVisitDataAsync(dbContext);
        var service = CreateService(dbContext);

        var visits = await service.GetListAsync("91020312345", null, CancellationToken.None);

        Assert.That(visits, Has.Count.EqualTo(1));
        Assert.That(visits[0].PatientFullName, Is.EqualTo("Anna Kowalska"));
        Assert.That(visits[0].PatientPESEL, Is.EqualTo("91020312345"));
    }

    [Test]
    public async Task GetDoctorScheduleAsync_ReturnsOnlyPlannedVisitsForSelectedDay()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        await SeedVisitDataAsync(dbContext);
        var service = CreateService(dbContext);

        var schedule = await service.GetDoctorScheduleAsync(1, new DateOnly(2026, 6, 18), CancellationToken.None);

        Assert.That(schedule, Has.Count.EqualTo(1));
        Assert.That(schedule[0].VisitStatus, Is.EqualTo(VisitState.Planned));
        Assert.That(schedule[0].DoctorFullName, Is.EqualTo("Adam Wisniewski"));
    }

    [Test]
    public async Task CreateAsync_WhenPatientAndDoctorExist_CreatesPlannedVisit()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        await TestData.SeedPeopleAsync(dbContext);
        var service = CreateService(dbContext);

        var visit = await service.CreateAsync(
            new CreateVisitDto
            {
                PatientId = 1,
                DoctorId = 1,
                VisitDateTime = new DateTime(2026, 7, 1, 12, 0, 0),
                Cost = 150m
            },
            CancellationToken.None);

        Assert.That(visit.VisitStatus, Is.EqualTo(VisitState.Planned));
        Assert.That(await dbContext.Visits.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdatePaymentAsync_WhenVisitExists_UpdatesPaidFlag()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        await SeedVisitDataAsync(dbContext);
        var service = CreateService(dbContext);

        var updated = await service.UpdatePaymentAsync(1, true, CancellationToken.None);

        var visit = await dbContext.Visits.FindAsync(1);
        Assert.That(updated, Is.True);
        Assert.That(visit!.IsPaid, Is.True);
    }

    private static VisitService CreateService(ClinicDbContext dbContext)
    {
        return new VisitService(
            dbContext,
            new VisitMapper(),
            NullLogger<VisitService>.Instance);
    }

    private static async Task SeedVisitDataAsync(ClinicDbContext dbContext)
    {
        await TestData.SeedPeopleAsync(dbContext);

        dbContext.Visits.AddRange(
            TestData.Visit(1, status: VisitState.Planned, dateTime: new DateTime(2026, 6, 18, 9, 0, 0)),
            TestData.Visit(2, patientId: 2, status: VisitState.InProgress, dateTime: new DateTime(2026, 6, 19, 10, 0, 0), cost: 300m),
            TestData.Visit(3, status: VisitState.Finished, dateTime: new DateTime(2026, 6, 18, 11, 0, 0), cost: 100m, isPaid: true));

        await dbContext.SaveChangesAsync();
    }
}
