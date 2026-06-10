using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IAuthenticationService
{
    Task<(bool Success, string[] Errors)> RegisterAsync(RegisterDto dto);
    Task<(bool Success, string ErrorMessage)> LoginAsync(string email, string password);
    Task LogoutAsync();
}