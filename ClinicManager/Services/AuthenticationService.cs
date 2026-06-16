using Microsoft.AspNetCore.Identity;
using ClinicManager.DTOs;

namespace ClinicManager.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<(bool Success, string[] Errors)> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Próba rejestracji na istniejący adres e-mail {Email}", dto.Email);
            return (false, new[] { "Email już zarejestrowany" });
        }

        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            _logger.LogWarning("Nie udało się zarejestrować użytkownika {Email}: {Errors}", dto.Email, string.Join("; ", errors));
            return (false, errors);
        }

        await _userManager.AddToRoleAsync(user, "Pacjent");

        _logger.LogInformation("Utworzono konto pacjenta {UserId}", user.Id);
        return (true, Array.Empty<string>());
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
