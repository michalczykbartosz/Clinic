using Microsoft.AspNetCore.Mvc;
using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Authorization;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin")]
public class AdminUsersController : Controller
{
    private readonly IUserManagementService _userManagementService;

    public AdminUsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
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
        if (dto is null) return NotFound();
        
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> EditRoles(string id, List<string> roleNames)
    {
        var (success, errorMessage) = await _userManagementService.EditRolesAsync(id, roleNames);
        if (success) return RedirectToAction("GetRoles");

        ModelState.AddModelError(string.Empty, errorMessage);
        
        var dto = await _userManagementService.GetUserRolesAsync(id);
        if (dto is null) return NotFound();
        
        return View(dto);
    }
}