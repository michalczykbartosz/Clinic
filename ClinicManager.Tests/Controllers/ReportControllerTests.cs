using ClinicManager.Controllers;
using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class ReportControllerTests
{
    [Test]
    public async Task Index_ReturnsView()
    {
        var controller = new ReportController(new StubReportService(), NullLogger<ReportController>.Instance);

        var result = controller.Index();

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task GetReportCost_WhenServiceSucceeds_ReturnsReportView()
    {
        var controller = new ReportController(
            new StubReportService { Result = (true, new ReportCostDto { OverallCost = 250m }, string.Empty) },
            NullLogger<ReportController>.Instance);

        var result = await controller.GetReportCost(null, null, new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30));

        var view = result as ViewResult;
        Assert.That(view, Is.Not.Null);
        Assert.That((view!.Model as ReportCostDto)!.OverallCost, Is.EqualTo(250m));
    }

    [Test]
    public async Task GetReportCost_WhenServiceFails_AddsModelError()
    {
        var controller = new ReportController(
            new StubReportService { Result = (false, null, "Blad raportu") },
            NullLogger<ReportController>.Instance);

        var result = await controller.GetReportCost(null, null, new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30));

        Assert.That(result, Is.InstanceOf<ViewResult>());
        Assert.That(controller.ModelState.IsValid, Is.False);
    }

    private sealed class StubReportService : IReportService
    {
        public (bool success, ReportCostDto? report, string error) Result { get; set; } = (true, new ReportCostDto(), string.Empty);

        public Task<(bool success, ReportCostDto?, string error)> GetReportCostAsync(
            int? patientId,
            int? doctorId,
            DateOnly startDate,
            DateOnly endDate,
            ReportCostScope scope,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result);
        }
    }
}
