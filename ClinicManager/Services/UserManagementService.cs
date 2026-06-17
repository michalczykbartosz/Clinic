using ClinicManager.DTOs;
using Microsoft.AspNetCore.Identity;

namespace ClinicManager.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<List<UserRolesDto>> GetAllUsersWithRolesAsync()
    {
        var result = new List<UserRolesDto>();
        var allUsers = _userManager.Users.ToList();

        foreach (var user in allUsers)
        {
            result.Add(new UserRolesDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            });
        }

        return result;
    }

    public async Task<UserRolesDto?> GetUserRolesAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("Nie znaleziono użytkownika {UserId} podczas pobierania ról.", id);
            return null;
        }

        return new UserRolesDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Roles = (await _userManager.GetRolesAsync(user)).ToList(),
            AllRoles = _roleManager.Roles
                .Select(role => role.Name)
                .Where(roleName => roleName != null)
                .Select(roleName => roleName!)
                .ToList()
        };
    }

    public async Task<(bool Success, string ErrorMessage)> EditRolesAsync(string id, List<string> roleNames)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("Nie znaleziono użytkownika {UserId} podczas zmiany ról.", id);
            return (false, "Użytkownik nie istnieje.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var result = await _userManager.AddToRolesAsync(user, roleNames);
        if (result.Succeeded)
        {
            _logger.LogInformation("Zmieniono role użytkownika {UserId} na {Roles}", id, string.Join(", ", roleNames));
            return (true, string.Empty);
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogError("Nie udało się zmienić ról użytkownika {UserId}: {Errors}", id, errors);
        return (false, errors);
    }

    public async Task<(bool Success, string ErrorMessage)> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("Nie znaleziono użytkownika {UserId} podczas usuwania konta.", id);
            return (false, "Użytkownik nie istnieje.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            _logger.LogInformation("Usunięto konto użytkownika {UserId} ({Email}).", id, user.Email);
            return (true, string.Empty);
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogError("Nie udało się usunąć konta użytkownika {UserId}: {Errors}", id, errors);
        return (false, errors);
    }
}
