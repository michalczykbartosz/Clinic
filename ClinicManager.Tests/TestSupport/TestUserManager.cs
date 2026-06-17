using System.ComponentModel.DataAnnotations;
using ClinicManager.Controllers;
using ClinicManager.Controllers.Api;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace ClinicManager.Tests;

internal sealed class TestUserManager : UserManager<IdentityUser>
{
    private readonly Dictionary<string, List<IdentityUser>> _usersByRole = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IList<Claim>> _claimsByUserId = new();

    public TestUserManager()
        : base(
            new TestUserStore(),
            null!,
            new PasswordHasher<IdentityUser>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<IdentityUser>>.Instance)
    {
    }

    public void AddUserToRole(string roleName, string pesel)
    {
        var user = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"{roleName}-{pesel}"
        };

        if (!_usersByRole.TryGetValue(roleName, out var users))
        {
            users = [];
            _usersByRole[roleName] = users;
        }

        users.Add(user);
        _claimsByUserId[user.Id] = [new Claim("PatientPesel", pesel)];
    }

    public override Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName)
    {
        var users = _usersByRole.TryGetValue(roleName, out var roleUsers)
            ? roleUsers.ToList()
            : [];

        return Task.FromResult<IList<IdentityUser>>(users);
    }

    public override Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
    {
        var claims = _claimsByUserId.TryGetValue(user.Id, out var userClaims)
            ? userClaims.ToList()
            : [];

        return Task.FromResult<IList<Claim>>(claims);
    }
}

internal sealed class TestUserStore : IUserStore<IdentityUser>
{
    public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public void Dispose()
    {
    }

    public Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IdentityUser?>(null);
    }

    public Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return Task.FromResult<IdentityUser?>(null);
    }

    public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(IdentityUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }
}

