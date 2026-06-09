using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ClinicManager.DTOs;

namespace ClinicManager.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        
        IdentityUser user = new IdentityUser();

        user.UserName = model.Email;
        user.Email = model.Email;

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Pacjent");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");

        }

        foreach (var oneError in result.Errors)
        {
            ModelState.AddModelError(string.Empty, oneError.Description);
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
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid) return View(model);
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);
        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError(string.Empty,"Nieprawidłowy login lub hasło!");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    
    
}