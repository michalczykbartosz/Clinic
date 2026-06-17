using System.Security.Claims;
using ClinicManager.Controllers;
using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class AccountControllerTests
{
    [Test]
    public async Task Register_Post_WhenServiceSucceeds_RedirectsToHome()
    {
        var authService = new StubAuthenticationService { RegisterResult = (true, []) };
        var controller = new AccountController(authService, NullLogger<AccountController>.Instance);

        var result = await controller.Register(NewRegisterDto());

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        Assert.That(redirect.ControllerName, Is.EqualTo("Home"));
        Assert.That(authService.RegisteredEmail, Is.EqualTo("patient@example.com"));
    }

    [Test]
    public async Task Login_Post_WhenServiceFails_ReturnsViewWithModelError()
    {
        var authService = new StubAuthenticationService { LoginResult = (false, "Bledne dane") };
        var controller = new AccountController(authService, NullLogger<AccountController>.Instance);
        var model = new LoginDto { Email = "patient@example.com", Password = "secret" };

        var result = await controller.Login(model);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        Assert.That(controller.ModelState.IsValid, Is.False);
    }

    [Test]
    public async Task Logout_CallsAuthenticationServiceAndRedirectsHome()
    {
        var authService = new StubAuthenticationService();
        var controller = new AccountController(authService, NullLogger<AccountController>.Instance)
        {
            ControllerContext = ControllerTestHelpers.ContextWithUser(new Claim(ClaimTypes.Name, "test-user"))
        };

        var result = await controller.Logout();

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        Assert.That(redirect.ControllerName, Is.EqualTo("Home"));
        Assert.That(authService.LogoutWasCalled, Is.True);
    }

    private static RegisterDto NewRegisterDto()
    {
        return new RegisterDto
        {
            FirstName = "Jan",
            LastName = "Nowak",
            Email = "patient@example.com",
            Password = "VerySecret123",
            PasswordConfirm = "VerySecret123",
            Pesel = "90051401234",
            InsuranceNumber = "NFZ-1",
            BirthDate = new DateOnly(1990, 5, 14)
        };
    }

    private sealed class StubAuthenticationService : IAuthenticationService
    {
        public (bool Success, string[] Errors) RegisterResult { get; set; } = (true, []);
        public (bool Success, string ErrorMessage) LoginResult { get; set; } = (true, string.Empty);
        public string? RegisteredEmail { get; private set; }
        public bool LogoutWasCalled { get; private set; }

        public Task<(bool Success, string[] Errors)> RegisterAsync(RegisterDto dto)
        {
            RegisteredEmail = dto.Email;
            return Task.FromResult(RegisterResult);
        }

        public Task<(bool Success, string ErrorMessage)> LoginAsync(string email, string password)
        {
            return Task.FromResult(LoginResult);
        }

        public Task LogoutAsync()
        {
            LogoutWasCalled = true;
            return Task.CompletedTask;
        }
    }
}
