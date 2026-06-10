using ClinicManager.DTOs;
using Microsoft.AspNetCore.Identity;

namespace ClinicManager.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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
                Email = user.Email,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            });
        }

        return result;
    }

    public async Task<UserRolesDto?> GetUserRolesAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return null;

        return new UserRolesDto
        {
            Id = user.Id,
            Email = user.Email,
            Roles = (await _userManager.GetRolesAsync(user)).ToList(),
            AllRoles = _roleManager.Roles.Select(x => x.Name).ToList()
        };
    }

    public async Task<(bool Success, string ErrorMessage)> EditRolesAsync(string id, List<string> roleNames)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return (false, "Użytkownik nie istnieje.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var result = await _userManager.AddToRolesAsync(user, roleNames);
        if (result.Succeeded) return (true, string.Empty);

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return (false, errors);
    }
}