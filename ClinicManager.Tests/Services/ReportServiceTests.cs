using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class ReportServiceTests
{
    [Test]
    public async Task GetReportCostAsync_SumsProceduresInSelectedDateRange()
    {
        await using var dbContext = await SeedReportDataAsync();
        var service = CreateService(dbContext);

        var result = await service.GetReportCostAsync(
            null,
            null,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 30),
            ReportCostScope.Procedures,
            CancellationToken.None);

        Assert.That(result.success, Is.True);
        Assert.That(result.Item2!.OverallCost, Is.EqualTo(300m));
    }

    [Test]
    public async Task GetReportCostAsync_WhenPatientFilterIsSet_SumsOnlyPatientProcedures()
    {
        await using var dbContext = await SeedReportDataAsync();
        var service = CreateService(dbContext);

        var result = await service.GetReportCostAsync(
            2,
            null,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 30),
            ReportCostScope.Procedures,
            CancellationToken.None);

        Assert.That(result.Item2!.OverallCost, Is.EqualTo(200m));
    }

    [Test]
    public async Task GetReportCostAsync_WhenDoctorFilterIsSet_SumsOnlyDoctorProcedures()
    {
        await using var dbContext = await SeedReportDataAsync();
        var service = CreateService(dbContext);

        var result = await service.GetReportCostAsync(
            null,
            2,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 30),
            ReportCostScope.Procedures,
            CancellationToken.None);

        Assert.That(result.Item2!.OverallCost, Is.EqualTo(200m));
    }

    private static ReportService CreateService(ClinicDbContext dbContext)
    {
        return new ReportService(dbContext, NullLogger<ReportService>.Instance);
    }

    private static async Task<TestClinicDbContext> SeedReportDataAsync()
    {
        var dbContext = await TestDbContextFactory.CreateAsync();
        await TestData.SeedPeopleAsync(dbContext);

        dbContext.MedicalRecords.AddRange(
            TestData.MedicalRecord(1, 1),
            TestData.MedicalRecord(2, 2));

        dbContext.Procedures.AddRange(
            TestData.Procedure(1, 1, 1, cost: 100m, date: new DateTime(2026, 6, 10)),
            TestData.Procedure(2, 2, 2, cost: 200m, date: new DateTime(2026, 6, 11)),
            TestData.Procedure(3, 1, 1, cost: 500m, date: new DateTime(2026, 7, 1)));

        await dbContext.SaveChangesAsync();
        return dbContext;
    }
}
