using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Lekarz")]
public class DoctorProfileController : Controller
{
    private readonly ClinicDbContext _dbContext;
    private readonly ILogger<DoctorProfileController> _logger;

    public DoctorProfileController(ClinicDbContext dbContext, ILogger<DoctorProfileController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
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

        doctor.PwzNumber = string.IsNullOrWhiteSpace(model.PwzNumber) ? "BRAK" : model.PwzNumber.Trim();
        doctor.Specialization = model.Specialization.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Lekarz {DoctorId} zaktualizował swój profil.", doctor.DoctorId);

        TempData["SuccessMessage"] = "Profil lekarza został zaktualizowany.";
        return RedirectToAction(nameof(Edit));
    }

    private async Task<Doctor?> GetOrCreateCurrentDoctorAsync(CancellationToken cancellationToken)
    {
        var pesel = User.FindFirstValue("PatientPesel")?.Trim();
        if (string.IsNullOrWhiteSpace(pesel))
        {
            return null;
        }

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

        _logger.LogInformation("Utworzono profil lekarza dla zalogowanego konta z PESEL {Pesel}.", pesel);
        return doctor;
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
