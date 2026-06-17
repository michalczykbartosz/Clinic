using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Lekarz")]
public class DoctorProfileController : Controller
{
    private readonly ClinicDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<DoctorProfileController> _logger;

    public DoctorProfileController(
        ClinicDbContext dbContext,
        UserManager<IdentityUser> userManager,
        ILogger<DoctorProfileController> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Lekarz")]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken)
    {
        var doctor = await GetOrCreateCurrentDoctorAsync(cancellationToken);
        if (doctor is null)
        {
            TempData["ErrorMessage"] = "Nie udało się powiązać konta lekarza z profilem. Konto musi mieć zapisany PESEL.";
            return RedirectToAction("Index", "Home");
        }

        return View(ToDto(doctor));
    }

    [HttpPost]
    [Authorize(Roles = "Lekarz")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateDoctorProfileDto model, CancellationToken cancellationToken)
    {
        var doctor = await GetOrCreateCurrentDoctorAsync(cancellationToken);
        if (doctor is null)
        {
            TempData["ErrorMessage"] = "Nie udało się powiązać konta lekarza z profilem. Konto musi mieć zapisany PESEL.";
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            model.DoctorId = doctor.DoctorId;
            model.FirstName = doctor.FirstName;
            model.LastName = doctor.LastName;
            model.PESEL = doctor.PESEL;
            return View(model);
        }

        UpdateDoctor(doctor, model);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Lekarz {DoctorId} zaktualizował swój profil.", doctor.DoctorId);

        TempData["SuccessMessage"] = "Profil lekarza został zaktualizowany.";
        return RedirectToAction(nameof(Edit));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditEmployee(string userId, CancellationToken cancellationToken)
    {
        var doctor = await GetOrCreateDoctorForUserAsync(userId, cancellationToken);
        if (doctor is null)
        {
            TempData["ErrorMessage"] = "Nie udało się powiązać konta lekarza z profilem. Konto musi mieć zapisany PESEL i kartotekę pacjenta.";
            return RedirectToAction("Employees", "AdminUsers");
        }

        ViewData["AdminEditUserId"] = userId;
        return View("Edit", ToDto(doctor));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEmployee(string userId, UpdateDoctorProfileDto model, CancellationToken cancellationToken)
    {
        var doctor = await GetOrCreateDoctorForUserAsync(userId, cancellationToken);
        if (doctor is null)
        {
            TempData["ErrorMessage"] = "Nie udało się powiązać konta lekarza z profilem. Konto musi mieć zapisany PESEL i kartotekę pacjenta.";
            return RedirectToAction("Employees", "AdminUsers");
        }

        if (!ModelState.IsValid)
        {
            model.DoctorId = doctor.DoctorId;
            model.FirstName = doctor.FirstName;
            model.LastName = doctor.LastName;
            model.PESEL = doctor.PESEL;
            ViewData["AdminEditUserId"] = userId;
            return View("Edit", model);
        }

        UpdateDoctor(doctor, model);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Administrator zaktualizował profil lekarza {DoctorId} dla konta {UserId}.", doctor.DoctorId, userId);

        TempData["SuccessMessage"] = "Profil lekarza został zaktualizowany.";
        return RedirectToAction("Employees", "AdminUsers");
    }

    private async Task<Doctor?> GetOrCreateCurrentDoctorAsync(CancellationToken cancellationToken)
    {
        var pesel = User.FindFirstValue("PatientPesel")?.Trim();
        if (string.IsNullOrWhiteSpace(pesel))
        {
            return null;
        }

        return await GetOrCreateDoctorByPeselAsync(pesel, cancellationToken);
    }

    private async Task<Doctor?> GetOrCreateDoctorForUserAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || !await _userManager.IsInRoleAsync(user, "Lekarz"))
        {
            return null;
        }

        var claims = await _userManager.GetClaimsAsync(user);
        var pesel = claims.FirstOrDefault(claim => claim.Type == "PatientPesel")?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(pesel))
        {
            return null;
        }

        return await GetOrCreateDoctorByPeselAsync(pesel, cancellationToken);
    }

    private async Task<Doctor?> GetOrCreateDoctorByPeselAsync(string pesel, CancellationToken cancellationToken)
    {
        var doctor = await _dbContext.Doctors
            .FirstOrDefaultAsync(existingDoctor => existingDoctor.PESEL == pesel, cancellationToken);

        if (doctor is not null)
        {
            return doctor;
        }

        var patient = await _dbContext.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(existingPatient => existingPatient.PESEL == pesel, cancellationToken);

        if (patient is null)
        {
            return null;
        }

        doctor = new Doctor
        {
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            PESEL = patient.PESEL,
            BirthDate = patient.BirthDate,
            PwzNumber = "BRAK",
            Specialization = "Brak specjalizacji"
        };

        _dbContext.Doctors.Add(doctor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Utworzono profil lekarza dla konta z PESEL {Pesel}.", pesel);
        return doctor;
    }

    private static void UpdateDoctor(Doctor doctor, UpdateDoctorProfileDto model)
    {
        doctor.PwzNumber = string.IsNullOrWhiteSpace(model.PwzNumber) ? "BRAK" : model.PwzNumber.Trim();
        doctor.Specialization = model.Specialization.Trim();
    }

    private static UpdateDoctorProfileDto ToDto(Doctor doctor)
    {
        return new UpdateDoctorProfileDto
        {
            DoctorId = doctor.DoctorId,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            PESEL = doctor.PESEL,
            PwzNumber = doctor.PwzNumber,
            Specialization = doctor.Specialization
        };
    }
}
