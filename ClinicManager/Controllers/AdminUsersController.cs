using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ClinicManager.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace ClinicManager.Controllers;

[Authorize(Roles="Admin")]
public class AdminUsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminUsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        List<UserRolesViewModel> dtoList = new List<UserRolesViewModel>();
        List<IdentityUser> allUsers = _userManager.Users.ToList();

        foreach (IdentityUser x in allUsers)
        {
            UserRolesViewModel model = new UserRolesViewModel();
            model.Roles = (await _userManager.GetRolesAsync(x)).ToList();
            model.Email = x.Email;
            model.Id = x.Id;
            dtoList.Add(model);
        }

        return View(dtoList);


    }

    [HttpGet]
    public async Task<IActionResult> EditRoles (string id)
    {
        IdentityUser wantedUser = await _userManager.FindByIdAsync(id);
        if (wantedUser is null) return NotFound();

        List<string> userRoles = (await _userManager.GetRolesAsync(wantedUser)).ToList();
        List<string> availableRoles = _roleManager.Roles.Select(x => x.Name).ToList();

        UserRolesViewModel model = new UserRolesViewModel();
        model.Id = id;
        model.Email = wantedUser.Email;
        model.Roles = userRoles;
        model.AllRoles = availableRoles;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditRoles(string id, List<string> roleNames)
    {
        var model = new UserRolesViewModel();
        IdentityUser wantedUser = await _userManager.FindByIdAsync(id);
        if (wantedUser is null) return NotFound();
        List<string> userRoles = (await _userManager.GetRolesAsync(wantedUser)).ToList();
        model.Id = id;
        model.Email = wantedUser.Email;
        model.Roles = userRoles;
        model.AllRoles = _roleManager.Roles.Select(x => x.Name).ToList();
        await _userManager.RemoveFromRolesAsync(wantedUser,userRoles);
        var result = await _userManager.AddToRolesAsync(wantedUser,roleNames);
        if(result.Succeeded) return RedirectToAction("GetRoles");
        foreach(var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty,error.Description); 
        }
        return View(model);


    }
    
}