using Microsoft.AspNetCore.Mvc;
using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin")]
public class AdminUsersController : Controller
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(IUserManagementService userManagementService, ILogger<AdminUsersController> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles(string roleFilter = "all")
    {
        var dtos = await _userManagementService.GetAllUsersWithRolesAsync();
        var normalizedFilter = NormalizeRoleFilter(roleFilter);

        var filteredDtos = dtos
            .Where(user => MatchesRoleFilter(user.Roles, normalizedFilter))
            .OrderBy(user => user.Email)
            .ToList();

        ViewData["RoleFilter"] = normalizedFilter;
        ViewData["TotalUsersCount"] = dtos.Count;
        return View(filteredDtos);
    }

    [HttpGet]
    public async Task<IActionResult> Employees()
    {
        var employeeRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Admin",
            "Lekarz",
            "Rejestratorka"
        };

        var dtos = await _userManagementService.GetAllUsersWithRolesAsync();
        var employees = (await _userManagementService.GetEmployeesAsync())
            .Where(user => user.Roles.Any(role => employeeRoles.Contains(role)))
            .ToList();

        ViewData["TotalUsersCount"] = dtos.Count;
        return View(employees);
    }

    [HttpGet]
    public async Task<IActionResult> EditRoles(string id)
    {
        var dto = await _userManagementService.GetUserRolesAsync(id);
        if (dto is null)
        {
            _logger.LogWarning("Nie znaleziono użytkownika {UserId} podczas edycji ról.", id);
            return NotFound();
        }
        
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> EditRoles(string id, List<string> roleNames)
    {
        var (success, errorMessage) = await _userManagementService.EditRolesAsync(id, roleNames);
        if (success)
        {
            _logger.LogInformation("Zmieniono role użytkownika {UserId}: {Roles}", id, string.Join(", ", roleNames));
            return RedirectToAction("GetRoles");
        }

        _logger.LogWarning("Nie udało się zmienić ról użytkownika {UserId}: {ErrorMessage}", id, errorMessage);
        ModelState.AddModelError(string.Empty, errorMessage);
        
        var dto = await _userManagementService.GetUserRolesAsync(id);
        if (dto is null)
        {
            _logger.LogWarning("Nie znaleziono użytkownika {UserId} po nieudanej zmianie ról.", id);
            return NotFound();
        }
        
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == id)
        {
            TempData["ErrorMessage"] = "Nie możesz usunąć konta, na którym jesteś aktualnie zalogowany.";
            return RedirectToAction(nameof(GetRoles));
        }

        var (success, errorMessage) = await _userManagementService.DeleteUserAsync(id);
        if (success)
        {
            _logger.LogInformation("Administrator {AdminId} usunął konto użytkownika {UserId}.", currentUserId, id);
            TempData["SuccessMessage"] = "Konto użytkownika zostało usunięte.";
            return RedirectToAction(nameof(GetRoles));
        }

        _logger.LogWarning("Nie udało się usunąć konta użytkownika {UserId}: {ErrorMessage}", id, errorMessage);
        TempData["ErrorMessage"] = errorMessage;
        return RedirectToAction(nameof(GetRoles));
    }

    private static string NormalizeRoleFilter(string roleFilter)
    {
        var allowedFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "all",
            "Admin",
            "Lekarz",
            "Rejestratorka",
            "Pacjent",
            "none"
        };

        return allowedFilters.Contains(roleFilter) ? roleFilter : "all";
    }

    private static bool MatchesRoleFilter(IReadOnlyCollection<string> roles, string roleFilter)
    {
        if (roleFilter == "all")
        {
            return true;
        }

        if (roleFilter == "none")
        {
            return !roles.Any();
        }

        return roles.Contains(roleFilter);
    }
}
