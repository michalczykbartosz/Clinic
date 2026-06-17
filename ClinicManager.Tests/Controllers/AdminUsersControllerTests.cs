using System.Security.Claims;
using ClinicManager.Controllers;
using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class AdminUsersControllerTests
{
    [Test]
    public async Task GetRoles_WhenRoleFilterIsSet_ReturnsOnlyMatchingUsers()
    {
        var controller = new AdminUsersController(
            new StubUserManagementService
            {
                Users =
                [
                    User("1", "admin@example.com", "Admin"),
                    User("2", "patient@example.com", "Pacjent")
                ]
            },
            NullLogger<AdminUsersController>.Instance);

        var result = await controller.GetRoles("Admin") as ViewResult;

        Assert.That(result, Is.Not.Null);
        var users = result!.Model as List<UserRolesDto>;
        Assert.That(users, Has.Count.EqualTo(1));
        Assert.That(users![0].Email, Is.EqualTo("admin@example.com"));
        Assert.That(result.ViewData["RoleFilter"], Is.EqualTo("Admin"));
    }

    [Test]
    public async Task EditRoles_Get_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var controller = new AdminUsersController(
            new StubUserManagementService(),
            NullLogger<AdminUsersController>.Instance);

        var result = await controller.EditRoles("missing");

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_WhenDeletingCurrentUser_RedirectsWithoutCallingService()
    {
        var service = new StubUserManagementService();
        var controller = new AdminUsersController(service, NullLogger<AdminUsersController>.Instance)
        {
            TempData = ControllerTestHelpers.TempData(),
            ControllerContext = ControllerTestHelpers.ContextWithUser(new Claim(ClaimTypes.NameIdentifier, "current"))
        };

        var result = await controller.Delete("current");

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("GetRoles"));
        Assert.That(service.DeletedUserId, Is.Null);
    }

    private static UserRolesDto User(string id, string email, params string[] roles)
    {
        return new UserRolesDto { Id = id, Email = email, Roles = roles.ToList() };
    }

    private sealed class StubUserManagementService : IUserManagementService
    {
        public List<UserRolesDto> Users { get; set; } = [];
        public UserRolesDto? User { get; set; }
        public string? DeletedUserId { get; private set; }

        public Task<List<UserRolesDto>> GetAllUsersWithRolesAsync()
        {
            return Task.FromResult(Users);
        }

        public Task<UserRolesDto?> GetUserRolesAsync(string id)
        {
            return Task.FromResult(User);
        }

        public Task<(bool Success, string ErrorMessage)> EditRolesAsync(string id, List<string> roleNames)
        {
            return Task.FromResult((true, string.Empty));
        }

        public Task<(bool Success, string ErrorMessage)> DeleteUserAsync(string id)
        {
            DeletedUserId = id;
            return Task.FromResult((true, string.Empty));
        }
    }
}
