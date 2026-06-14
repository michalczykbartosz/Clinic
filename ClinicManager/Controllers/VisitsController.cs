using ClinicManager.DTOs;
using ClinicManager.Models;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicManager.Controllers;

[Authorize]
public class VisitsController : Controller
{
    private readonly IVisitService _visitService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly ILogger<VisitsController> _logger;

    public VisitsController(
        IVisitService visitService,
        IPatientService patientService,
        IDoctorService doctorService,
        ILogger<VisitsController> logger)
    {
        _visitService = visitService;
        _patientService = patientService;
        _doctorService = doctorService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var visits = await _visitService.GetListAsync(cancellationToken);
        return View(visits);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await BuildCreateVisitViewModelAsync(new CreateVisitViewModel(), cancellationToken);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Rejestratorka")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateVisitViewModel model, CancellationToken cancellationToken)
    {
        if (model.Visit.VisitDateTime <= DateTime.Now)
        {
            ModelState.AddModelError(
                "Visit.VisitDateTime",
                "Data wizyty musi być późniejsza niż obecna.");
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await BuildCreateVisitViewModelAsync(model, cancellationToken);
            return View(invalidModel);
        }

        try
        {
            await _visitService.CreateAsync(model.Visit, cancellationToken);
            TempData["SuccessMessage"] = "Wizyta została utworzona.";

            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException exception)
        {
            _logger.LogWarning(exception, "Nie udało się utworzyć wizyty z powodu brakujących danych referencyjnych.");
            ModelState.AddModelError(string.Empty, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Nie udało się utworzyć wizyty.");
            ModelState.AddModelError(string.Empty, "Nie udało się utworzyć wizyty. Spróbuj ponownie.");
        }

        var errorModel = await BuildCreateVisitViewModelAsync(model, cancellationToken);
        return View(errorModel);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, UpdateVisitStatusDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !Enum.IsDefined(typeof(VisitState), model.VisitStatus))
        {
            TempData["ErrorMessage"] = "Wybrano nieprawidłowy status wizyty.";
            return RedirectToAction(nameof(Index));
        }

        var updated = await _visitService.UpdateStatusAsync(id, model.VisitStatus, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Status wizyty został zaktualizowany.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<CreateVisitViewModel> BuildCreateVisitViewModelAsync(
        CreateVisitViewModel model,
        CancellationToken cancellationToken)
    {
        var patients = await _patientService.GetAllAsync(cancellationToken);
        var doctors = await _doctorService.GetAllAsync(cancellationToken);

        model.Patients = patients
            .Select(patient => new SelectListItem
            {
                Value = patient.PatientId.ToString(),
                Text = $"{patient.LastName} {patient.FirstName} - {patient.PESEL}"
            })
            .ToList();

        model.Doctors = doctors
            .Select(doctor => new SelectListItem
            {
                Value = doctor.DoctorId.ToString(),
                Text = $"{doctor.LastName} {doctor.FirstName} - {doctor.Specialization}"
            })
            .ToList();

        return model;
    }
}
