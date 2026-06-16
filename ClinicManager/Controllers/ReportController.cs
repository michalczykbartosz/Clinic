using ClinicManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace ClinicManager.Controllers;

public class ReportController : Controller
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService reportService, ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
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
            _logger.LogError(
                "Nie udało się wygenerować raportu kosztów. PatientId={PatientId}, DoctorId={DoctorId}, StartDate={StartDate}, EndDate={EndDate}, Error={Error}",
                patientId,
                doctorId,
                startDate,
                endDate,
                error);
            ModelState.AddModelError(string.Empty,"Wystąpił błąd podczas pobierania danych!");
            return View();
        }

        _logger.LogInformation(
            "Wygenerowano raport kosztów. PatientId={PatientId}, DoctorId={DoctorId}, StartDate={StartDate}, EndDate={EndDate}",
            patientId,
            doctorId,
            startDate,
            endDate);
        return View(raport);
    }
}
