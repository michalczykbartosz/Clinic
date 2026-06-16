using ClinicManager.DTOs;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.Controllers;

[Authorize]
public class VisitMedicationsController : Controller
{
    private readonly IVisitMedicationService _visitMedicationService;

    public VisitMedicationsController(IVisitMedicationService visitMedicationService)
    {
        _visitMedicationService = visitMedicationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
    public async Task<IActionResult> Index(int visitId, CancellationToken cancellationToken)
    {
        var model = await _visitMedicationService.GetForVisitAsync(visitId, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Lekarz")]
    public async Task<IActionResult> Create(int visitId, CancellationToken cancellationToken)
    {
        var model = await _visitMedicationService.BuildCreateModelAsync(visitId, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Lekarz")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateVisitMedicationViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidModel = await _visitMedicationService.BuildCreateModelAsync(model.Medication.VisitId, cancellationToken);
            if (invalidModel is null)
            {
                return NotFound();
            }

            invalidModel.Medication = model.Medication;
            return View(invalidModel);
        }

        var visitId = await _visitMedicationService.AddMedicationAsync(model.Medication, cancellationToken);
        if (visitId is null)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Lek został przypisany do wizyty.";
        return RedirectToAction(nameof(Index), new { visitId });
    }
}
