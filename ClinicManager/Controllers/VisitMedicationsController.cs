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
    private readonly ILogger<VisitMedicationsController> _logger;

    public VisitMedicationsController(
        IVisitMedicationService visitMedicationService,
        ILogger<VisitMedicationsController> logger)
    {
        _visitMedicationService = visitMedicationService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
    public async Task<IActionResult> Index(int visitId, CancellationToken cancellationToken)
    {
        var model = await _visitMedicationService.GetForVisitAsync(visitId, cancellationToken);
        if (model is null)
        {
            _logger.LogWarning("Nie znaleziono wizyty {VisitId} podczas pobierania przypisanych leków.", visitId);
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Lekarz")]
    public async Task<IActionResult> Create(int visitId, CancellationToken cancellationToken)
    {
        var model = await _visitMedicationService.BuildCreateModelAsync(visitId, cancellationToken);
        if (model is null)
        {
            _logger.LogWarning("Nie znaleziono wizyty {VisitId} podczas otwierania formularza przypisania leku.", visitId);
            return NotFound();
        }

        return View(model);
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
                _logger.LogWarning(
                    "Nie znaleziono wizyty {VisitId} podczas ponownego wyświetlania formularza przypisania leku.",
                    model.Medication.VisitId);

                return NotFound();
            }

            invalidModel.Medication = model.Medication;
            return View(invalidModel);
        }

        var visitId = await _visitMedicationService.AddMedicationAsync(model.Medication, cancellationToken);
        if (visitId is null)
        {
            _logger.LogWarning(
                "Nie udało się przypisać leku {MedicationId} do wizyty {VisitId}.",
                model.Medication.MedicationId,
                model.Medication.VisitId);

            return NotFound();
        }

        _logger.LogInformation(
            "Przypisano lek {MedicationId} do wizyty {VisitId}.",
            model.Medication.MedicationId,
            visitId.Value);

        TempData["SuccessMessage"] = "Lek został przypisany do wizyty.";
        return RedirectToAction(nameof(Index), new { visitId });
    }
}
