using Microsoft.AspNetCore.Mvc;
using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Authorization;

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
    public async Task<IActionResult> GetRoles()
    {
        var dtos = await _userManagementService.GetAllUsersWithRolesAsync();
        return View(dtos);
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
}
