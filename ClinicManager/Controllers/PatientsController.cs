using ClinicManager.DTOs;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicManager.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly IPatientService _patientService;
    private readonly IVisitService _visitService;
    private readonly IPatientDocumentService _documentService;
    private readonly IUserManagementService _userManagementService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(
        IPatientService patientService,
        IVisitService visitService,
        IPatientDocumentService documentService,
        IUserManagementService userManagementService,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _visitService = visitService;
        _documentService = documentService;
        _userManagementService = userManagementService;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
    public async Task<IActionResult> Index(string? query, CancellationToken cancellationToken)
    {
        ViewData["Query"] = query;

        var patients = await _patientService.SearchAsync(query, cancellationToken);
        var employeePesels = await _userManagementService.GetEmployeePatientPeselsAsync();
        patients = patients
            .Where(patient => !employeePesels.Contains(patient.PESEL))
            .ToList();

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

        PatientDto patient;
        try
        {
            patient = await _patientService.CreateAsync(model, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }

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

        bool updated;
        try
        {
            updated = await _patientService.UpdateAsync(id, model, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }

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
    [Authorize(Roles = "Pacjent")]
    public async Task<IActionResult> MyData(CancellationToken cancellationToken)
    {
        var patient = await GetCurrentPatientAsync(cancellationToken);
        if (patient is null)
        {
            ViewData["PatientAccessMessage"] = "Nie udało się znaleźć kartoteki powiązanej z Twoim kontem.";
            return View(new UpsertPatientDto());
        }

        return View(ToUpsertPatientDto(patient));
    }

    [HttpPost]
    [Authorize(Roles = "Pacjent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MyData(UpsertPatientDto model, CancellationToken cancellationToken)
    {
        var patient = await GetCurrentPatientAsync(cancellationToken);
        if (patient is null)
        {
            ModelState.AddModelError(string.Empty, "Nie udało się znaleźć kartoteki powiązanej z Twoim kontem.");
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var previousPesel = patient.PESEL;
        try
        {
            var updated = await _patientService.UpdateAsync(patient.PatientId, model, cancellationToken);
            if (!updated)
            {
                return NotFound();
            }
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }

        if (!string.Equals(previousPesel, model.PESEL.Trim(), StringComparison.Ordinal))
        {
            await UpdatePatientPeselClaimAsync(model.PESEL.Trim());
        }

        TempData["SuccessMessage"] = "Twoje dane zostały zaktualizowane.";
        return RedirectToAction(nameof(MyData));
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

    private async Task<PatientDto?> GetCurrentPatientAsync(CancellationToken cancellationToken)
    {
        var patientPesel = User.FindFirstValue("PatientPesel");
        if (string.IsNullOrWhiteSpace(patientPesel))
        {
            return null;
        }

        return await _patientService.GetByPeselAsync(patientPesel, cancellationToken);
    }

    private async Task UpdatePatientPeselClaimAsync(string pesel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return;
        }

        var claims = await _userManager.GetClaimsAsync(user);
        var existingClaim = claims.FirstOrDefault(claim => claim.Type == "PatientPesel");
        var newClaim = new Claim("PatientPesel", pesel);

        if (existingClaim is null)
        {
            await _userManager.AddClaimAsync(user, newClaim);
        }
        else
        {
            await _userManager.ReplaceClaimAsync(user, existingClaim, newClaim);
        }

        await _signInManager.RefreshSignInAsync(user);
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
