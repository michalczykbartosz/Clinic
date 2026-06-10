using Microsoft.AspNetCore.Mvc;
using ClinicManager.DTOs;
using ClinicManager.Services;

namespace ClinicManager.Controllers;

public class AccountController : Controller
{
    private readonly IAuthenticationService _authService;

    public AccountController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid) return View(model);
        
        var (success, errors) = await _authService.RegisterAsync(model);
        
        if (success) return RedirectToAction("Index", "Home");

        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid) return View(model);
        
        var (success, errorMessage) = await _authService.LoginAsync(model.Email, model.Password);
        
        if (success)
        {
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, errorMessage);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }
}