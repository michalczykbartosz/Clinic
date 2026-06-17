using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Models;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManager.Controllers;

[Authorize]
public class VisitsController : Controller
{
    private readonly IVisitService _visitService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ClinicDbContext _dbContext;
    private readonly ILogger<VisitsController> _logger;

    public VisitsController(
        IVisitService visitService,
        UserManager<IdentityUser> userManager,
        ClinicDbContext dbContext,
        ILogger<VisitsController> logger)
    {
        _visitService = visitService;
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Rejestratorka,Lekarz,Pacjent")]
    public async Task<IActionResult> Index(string? query, VisitState? status, CancellationToken cancellationToken)
    {
        var isPatient = User.IsInRole("Pacjent");
        ViewData["IsPatientView"] = isPatient;
        ViewData["Query"] = query;
        ViewData["Status"] = status;

        if (isPatient)
        {
            ViewData["Query"] = string.Empty;

            var patientPesel = User.FindFirstValue("PatientPesel");
            if (string.IsNullOrWhiteSpace(patientPesel))
            {
                ViewData["PatientAccessMessage"] = "Nie udało się powiązać konta z kartoteką pacjenta. Zarejestruj konto z poprawnym numerem PESEL albo poproś recepcję o sprawdzenie danych.";
                return View(Array.Empty<VisitListItemDto>());
            }

            var patientVisits = await _visitService.GetListForPatientPeselAsync(patientPesel, status, cancellationToken);
            return View(patientVisits);
        }

        var visits = await _visitService.GetListAsync(query, status, cancellationToken);
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
    public async Task<IActionResult> UpdateStatus(int id, UpdateVisitStatusDto model, string? query, VisitState? status, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !Enum.IsDefined(typeof(VisitState), model.VisitStatus))
        {
            TempData["ErrorMessage"] = "Wybrano nieprawidłowy status wizyty.";
            return RedirectToAction(nameof(Index), new { query, status });
        }

        var updated = await _visitService.UpdateStatusAsync(id, model.VisitStatus, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Status wizyty został zaktualizowany.";
        return RedirectToAction(nameof(Index), new { query, status });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Rejestratorka")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePayment(int id, string? query, VisitState? status, CancellationToken cancellationToken)
    {
        var isPaid = Request.Form["isPaid"].Any(value =>
            string.Equals(value, "true", StringComparison.OrdinalIgnoreCase));

        var updated = await _visitService.UpdatePaymentAsync(id, isPaid, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Płatność wizyty została zaktualizowana.";
        return RedirectToAction(nameof(Index), new { query, status });
    }

    private async Task<CreateVisitViewModel> BuildCreateVisitViewModelAsync(
        CreateVisitViewModel model,
        CancellationToken cancellationToken)
    {
        var patients = await GetVisitPatientsAsync(cancellationToken);
        var doctors = await GetAvailableDoctorsAsync(cancellationToken);

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

    private async Task<IReadOnlyList<PatientDto>> GetVisitPatientsAsync(CancellationToken cancellationToken)
    {
        var employeePesels = new HashSet<string>();
        foreach (var role in new[] { "Admin", "Lekarz", "Rejestratorka" })
        {
            employeePesels.UnionWith(await GetRoleUserPeselsAsync(role));
        }

        var patients = await _dbContext.Patients
            .AsNoTracking()
            .Where(patient => !employeePesels.Contains(patient.PESEL))
            .OrderBy(patient => patient.LastName)
            .ThenBy(patient => patient.FirstName)
            .Select(patient => new PatientDto
            {
                PatientId = patient.PatientId,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                PESEL = patient.PESEL,
                InsuranceNumber = patient.InsuranceNumber,
                BirthDate = patient.BirthDate
            })
            .ToListAsync(cancellationToken);

        return patients;
    }

    private async Task<IReadOnlyList<DoctorDto>> GetAvailableDoctorsAsync(CancellationToken cancellationToken)
    {
        var doctorPesels = await GetRoleUserPeselsAsync("Lekarz");
        if (doctorPesels.Count > 0)
        {
            await EnsureDoctorProfilesForDoctorAccountsAsync(doctorPesels, cancellationToken);
        }

        var doctors = await _dbContext.Doctors
            .AsNoTracking()
            .OrderBy(doctor => doctor.LastName)
            .ThenBy(doctor => doctor.FirstName)
            .Select(doctor => new DoctorDto
            {
                DoctorId = doctor.DoctorId,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                PESEL = doctor.PESEL,
                BirthDate = doctor.BirthDate,
                PwzNumber = doctor.PwzNumber,
                Specialization = doctor.Specialization
            })
            .ToListAsync(cancellationToken);

        return doctors;
    }

    private async Task<HashSet<string>> GetRoleUserPeselsAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        var pesels = new HashSet<string>();

        foreach (var user in users)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var pesel = claims.FirstOrDefault(claim => claim.Type == "PatientPesel")?.Value;
            if (!string.IsNullOrWhiteSpace(pesel))
            {
                pesels.Add(pesel.Trim());
            }
        }

        return pesels;
    }

    private async Task EnsureDoctorProfilesForDoctorAccountsAsync(
        IReadOnlySet<string> doctorPesels,
        CancellationToken cancellationToken)
    {
        var existingDoctorPesels = await _dbContext.Doctors
            .Where(doctor => doctorPesels.Contains(doctor.PESEL))
            .Select(doctor => doctor.PESEL)
            .ToListAsync(cancellationToken);

        var missingDoctorPesels = doctorPesels.Except(existingDoctorPesels).ToList();
        if (missingDoctorPesels.Count == 0)
        {
            return;
        }

        var sourcePatients = await _dbContext.Patients
            .AsNoTracking()
            .Where(patient => missingDoctorPesels.Contains(patient.PESEL))
            .ToListAsync(cancellationToken);

        foreach (var patient in sourcePatients)
        {
            _dbContext.Doctors.Add(new Doctor
            {
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                PESEL = patient.PESEL,
                BirthDate = patient.BirthDate,
                PwzNumber = "BRAK",
                Specialization = "Brak specjalizacji"
            });
        }

        if (sourcePatients.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Utworzono brakujące profile lekarzy dla {DoctorProfilesCount} kont z rolą Lekarz.", sourcePatients.Count);
        }
    }
}
