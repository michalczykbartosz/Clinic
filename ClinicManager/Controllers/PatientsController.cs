using ClinicManager.DTOs;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly IPatientService _patientService;
    private readonly IVisitService _visitService;
    private readonly IPatientDocumentService _documentService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(
        IPatientService patientService,
        IVisitService visitService,
        IPatientDocumentService documentService,
        ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _visitService = visitService;
        _documentService = documentService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
    public async Task<IActionResult> Index(string? query, CancellationToken cancellationToken)
    {
        ViewData["Query"] = query;

        var patients = await _patientService.SearchAsync(query, cancellationToken);
        return View(patients);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        var visits = await _visitService.GetByPatientIdAsync(id, cancellationToken);
        IReadOnlyList<PatientDocumentDto> documents = Array.Empty<PatientDocumentDto>();

        try
        {
            documents = await _documentService.GetByPatientIdAsync(id, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Nie udało się pobrać dokumentów pacjenta {PatientId}", id);
            TempData["ErrorMessage"] = "Pacjent został zapisany, ale nie udało się załadować dokumentów. Sprawdź, czy migracje bazy danych są aktualne.";
        }

        var model = new PatientDetailsViewModel
        {
            Patient = patient,
            Visits = visits,
            Documents = documents
        };

        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka")]
    public IActionResult Create()
    {
        return View(new UpsertPatientDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Rejestratorka")]
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
    [Authorize(Roles = "Admin,Rejestratorka")]
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
    [Authorize(Roles = "Admin,Rejestratorka")]
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
    [Authorize(Roles = "Admin,Rejestratorka")]
    public async Task<IActionResult> Record(int id, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        SetRecordViewData(patient);
        return View(ToUpdatePatientRecordDto(patient));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Rejestratorka")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(int id, UpdatePatientRecordDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var patient = await _patientService.GetByIdAsync(id, cancellationToken);
            if (patient is null)
            {
                return NotFound();
            }

            SetRecordViewData(patient);
            return View(model);
        }

        var updated = await _patientService.UpdateRecordAsync(id, model, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Kartoteka pacjenta została uzupełniona.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka")]
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
    [Authorize(Roles = "Admin,Rejestratorka")]
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

    private static UpdatePatientRecordDto ToUpdatePatientRecordDto(PatientDto patient)
    {
        return new UpdatePatientRecordDto
        {
            PESEL = patient.PESEL,
            InsuranceNumber = patient.InsuranceNumber
        };
    }

    private void SetRecordViewData(PatientDto patient)
    {
        ViewData["PatientId"] = patient.PatientId;
        ViewData["PatientFullName"] = $"{patient.FirstName} {patient.LastName}";
    }
}
