using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.Controllers;

[Authorize]
public class ProceduresController : Controller
{
    private readonly IProcedureService _procedureService;

    public ProceduresController(IProcedureService procedureService)
    {
        _procedureService = procedureService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
    public async Task<IActionResult> Index(int visitId, CancellationToken cancellationToken)
    {
        var model = await _procedureService.GetForVisitAsync(visitId, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Lekarz")]
    public async Task<IActionResult> Create(int visitId, CancellationToken cancellationToken)
    {
        var model = await _procedureService.BuildCreateModelAsync(visitId, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Lekarz")]
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
            return NotFound();
        }

        TempData["SuccessMessage"] = "Procedura została dodana.";
        return RedirectToAction(nameof(Index), new { visitId });
    }
}
