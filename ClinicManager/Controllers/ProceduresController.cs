using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.Controllers;

[Authorize]
public class ProceduresController : Controller
{
    private readonly IProcedureService _procedureService;
    private readonly ILogger<ProceduresController> _logger;

    public ProceduresController(IProcedureService procedureService, ILogger<ProceduresController> logger)
    {
        _procedureService = procedureService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
    public async Task<IActionResult> Index(int visitId, CancellationToken cancellationToken)
    {
        var model = await _procedureService.GetForVisitAsync(visitId, cancellationToken);
        if (model is null)
        {
            _logger.LogWarning("Nie znaleziono wizyty {VisitId} podczas pobierania procedur.", visitId);
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Lekarz")]
    public async Task<IActionResult> Create(int visitId, CancellationToken cancellationToken)
    {
        var model = await _procedureService.BuildCreateModelAsync(visitId, cancellationToken);
        if (model is null)
        {
            _logger.LogWarning("Nie znaleziono wizyty {VisitId} podczas tworzenia procedury.", visitId);
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Lekarz")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProcedureDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var visitId = await _procedureService.CreateAsync(model, cancellationToken);
        if (visitId is null)
        {
            _logger.LogWarning("Nie udało się dodać procedury do wizyty {VisitId}.", model.VisitId);
            return NotFound();
        }

        _logger.LogInformation("Dodano procedurę do wizyty {VisitId}", visitId);
        TempData["SuccessMessage"] = "Procedura została dodana.";
        return RedirectToAction(nameof(Index), new { visitId });
    }
}
