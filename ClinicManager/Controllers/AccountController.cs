using Microsoft.AspNetCore.Mvc;
using ClinicManager.DTOs;
using ClinicManager.Services;

namespace ClinicManager.Controllers;

public class AccountController : Controller
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthenticationService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Nieudana rejestracja użytkownika {Email}: formularz zawiera niepoprawne dane.", model.Email);
            return View(model);
        }
        
        var (success, errors) = await _authService.RegisterAsync(model);
        
        if (success)
        {
            _logger.LogInformation("Zarejestrowano użytkownika {Email}", model.Email);
            return RedirectToAction("Index", "Home");
        }

        _logger.LogWarning("Nieudana rejestracja użytkownika {Email}: {Errors}", model.Email, string.Join("; ", errors));

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
            _logger.LogInformation("Zalogowano użytkownika {Email}", model.Email);
            return RedirectToAction("Index", "Home");
        }

        _logger.LogWarning("Nieudana próba logowania użytkownika {Email}", model.Email);
        ModelState.AddModelError(string.Empty, errorMessage);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var userName = User.Identity?.Name;
        await _authService.LogoutAsync();
        _logger.LogInformation("Wylogowano użytkownika {UserName}", userName);
        return RedirectToAction("Index", "Home");
    }
}
