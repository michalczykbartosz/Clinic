using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManager.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ClinicDbContext _dbContext;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ClinicDbContext dbContext,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<(bool Success, string[] Errors)> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim();
        var pesel = dto.Pesel.Trim();
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            _logger.LogWarning("Próba rejestracji na istniejący adres e-mail {Email}", email);
            return (false, ["Email już zarejestrowany."]);
        }

        var user = new IdentityUser
        {
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => error.Description).ToArray();
            _logger.LogWarning("Nie udało się zarejestrować użytkownika {Email}: {Errors}", email, string.Join("; ", errors));
            return (false, errors);
        }

        var claimResult = await _userManager.AddClaimAsync(user, new Claim("PatientPesel", pesel));
        if (!claimResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            var errors = claimResult.Errors.Select(error => error.Description).ToArray();
            _logger.LogWarning("Nie udało się zapisać PESEL użytkownika {Email}: {Errors}", email, string.Join("; ", errors));
            return (false, errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Pacjent");
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            var errors = roleResult.Errors.Select(error => error.Description).ToArray();
            _logger.LogWarning("Nie udało się nadać roli Pacjent użytkownikowi {Email}: {Errors}", email, string.Join("; ", errors));
            return (false, errors);
        }

        var patientExists = await _dbContext.Patients.AnyAsync(patient => patient.PESEL == pesel);
        if (!patientExists)
        {
            _dbContext.Patients.Add(new Patient
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                PESEL = pesel,
                InsuranceNumber = dto.InsuranceNumber.Trim(),
                BirthDate = dto.BirthDate
            });

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                await _userManager.DeleteAsync(user);
                _logger.LogError(exception, "Nie udało się utworzyć kartoteki pacjenta podczas rejestracji {Email}.", email);
                return (false, ["Nie udało się utworzyć kartoteki pacjenta. Spróbuj ponownie."]);
            }
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        _logger.LogInformation("Utworzono i zalogowano konto pacjenta {UserId}", user.Id);
        return (true, []);
    }

    public async Task<(bool Success, string ErrorMessage)> LoginAsync(string email, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("Pomyślne logowanie użytkownika {Email}", email);
            return (true, string.Empty);
        }

        _logger.LogWarning("Nieudane logowanie użytkownika {Email}", email);
        return (false, "Nieprawidłowy login lub hasło");
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Wylogowano bieżącego użytkownika.");
    }
}
