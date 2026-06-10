using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Rejestratorka")]
public class PatientsController : Controller
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var patients = await _patientService.GetAllAsync(cancellationToken);
        return View(patients);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        return View(patient);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new UpsertPatientDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UpsertPatientDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var patient = await _patientService.CreateAsync(model, cancellationToken);
        TempData["SuccessMessage"] = "Pacjent został dodany.";

        return RedirectToAction(nameof(Details), new { id = patient.PatientId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        return View(ToUpsertPatientDto(patient));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpsertPatientDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await _patientService.UpdateAsync(id, model, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Dane pacjenta zostały zaktualizowane.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        return View(patient);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _patientService.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Pacjent został usunięty.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Nie udało się usunąć pacjenta {PatientId}", id);
            ModelState.AddModelError(string.Empty, "Nie udało się usunąć pacjenta. Sprawdź, czy pacjent nie ma powiązanych wizyt lub dokumentacji.");

            var patient = await _patientService.GetByIdAsync(id, cancellationToken);
            if (patient is null)
            {
                return NotFound();
            }

            return View(patient);
        }
    }

    private static UpsertPatientDto ToUpsertPatientDto(PatientDto patient)
    {
        return new UpsertPatientDto
        {
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            PESEL = patient.PESEL,
            InsuranceNumber = patient.InsuranceNumber,
            BirthDate = patient.BirthDate
        };
    }
}
