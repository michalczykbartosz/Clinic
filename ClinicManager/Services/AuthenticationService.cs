using Microsoft.AspNetCore.Identity;
using ClinicManager.DTOs;

namespace ClinicManager.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AuthenticationService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<(bool Success, string[] Errors)> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null) return (false, new[] { "Email już zarejestrowany" });

        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return (false, errors);
        }

        await _userManager.AddToRoleAsync(user, "Pacjent");

        return (true, Array.Empty<string>());
    }

    public async Task<(bool Success, string ErrorMessage)> LoginAsync(string email, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: true);
        if (result.Succeeded) return (true, string.Empty);

        return (false, "Nieprawidłowy login lub hasło");
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}