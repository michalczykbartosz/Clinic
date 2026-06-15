using ClinicManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace ClinicManager.Controllers;

public class ReportController : Controller
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    [Authorize(Roles="Admin")]
    public async Task<IActionResult> Index()
    {
        return View();
    }
    
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetReportCost(int? patientId, int? doctorId,
        DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var (success, raport, error) =
            await _reportService.GetReportCostAsync(patientId, doctorId, startDate, endDate, cancellationToken);

        if (!success)
        {
            ModelState.AddModelError(string.Empty,"Wystąpił błąd podczas pobierania danych!");
            return View();
        }

        return View(raport);
    }
}