using ClinicManager.Data;
using ClinicManager.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManager.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ClinicDbContext _dbContext;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ClinicDbContext dbContext,
        ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<UserRolesDto>> GetAllUsersWithRolesAsync()
    {
        var result = new List<UserRolesDto>();
        var allUsers = _userManager.Users.ToList();

        foreach (var user in allUsers)
        {
            result.Add(new UserRolesDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            });
        }

        return result;
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync()
    {
        var employeeRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Admin",
            "Lekarz",
            "Rejestratorka"
        };

        var userRows = new List<(IdentityUser User, List<string> Roles, string Pesel)>();
        var allUsers = _userManager.Users.ToList();

        foreach (var user in allUsers)
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            if (!roles.Any(role => employeeRoles.Contains(role)))
            {
                continue;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var pesel = claims.FirstOrDefault(claim => claim.Type == "PatientPesel")?.Value?.Trim() ?? string.Empty;
            userRows.Add((user, roles, pesel));
        }

        var pesels = userRows
            .Select(row => row.Pesel)
            .Where(pesel => !string.IsNullOrWhiteSpace(pesel))
            .ToHashSet();

        var patientsByPesel = await _dbContext.Patients
            .AsNoTracking()
            .Where(patient => pesels.Contains(patient.PESEL))
            .ToDictionaryAsync(patient => patient.PESEL);

        var doctorsByPesel = await _dbContext.Doctors
            .AsNoTracking()
            .Where(doctor => pesels.Contains(doctor.PESEL))
            .ToDictionaryAsync(doctor => doctor.PESEL);

        return userRows
            .Select(row =>
            {
                patientsByPesel.TryGetValue(row.Pesel, out var patient);
                doctorsByPesel.TryGetValue(row.Pesel, out var doctor);

                return new EmployeeDto
                {
                    Id = row.User.Id,
                    Email = row.User.Email ?? string.Empty,
                    FirstName = patient?.FirstName ?? doctor?.FirstName ?? string.Empty,
                    LastName = patient?.LastName ?? doctor?.LastName ?? string.Empty,
                    Pesel = row.Pesel,
                    DoctorId = doctor?.DoctorId,
                    Roles = row.Roles
                };
            })
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ThenBy(employee => employee.Email)
            .ToList();
    }

    public async Task<UserRolesDto?> GetUserRolesAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("Nie znaleziono użytkownika {UserId} podczas pobierania ról.", id);
            return null;
        }

        return new UserRolesDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Roles = (await _userManager.GetRolesAsync(user)).ToList(),
            AllRoles = _roleManager.Roles
                .Select(role => role.Name)
                .Where(roleName => roleName != null)
                .Select(roleName => roleName!)
                .ToList()
        };
    }

    public async Task<(bool Success, string ErrorMessage)> EditRolesAsync(string id, List<string> roleNames)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("Nie znaleziono użytkownika {UserId} podczas zmiany ról.", id);
            return (false, "Użytkownik nie istnieje.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var result = await _userManager.AddToRolesAsync(user, roleNames);
        if (result.Succeeded)
        {
            _logger.LogInformation("Zmieniono role użytkownika {UserId} na {Roles}", id, string.Join(", ", roleNames));
            return (true, string.Empty);
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogError("Nie udało się zmienić ról użytkownika {UserId}: {Errors}", id, errors);
        return (false, errors);
    }

    public async Task<(bool Success, string ErrorMessage)> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("Nie znaleziono użytkownika {UserId} podczas usuwania konta.", id);
            return (false, "Użytkownik nie istnieje.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            _logger.LogInformation("Usunięto konto użytkownika {UserId} ({Email}).", id, user.Email);
            return (true, string.Empty);
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogError("Nie udało się usunąć konta użytkownika {UserId}: {Errors}", id, errors);
        return (false, errors);
    }

    public async Task<HashSet<string>> GetEmployeePatientPeselsAsync()
    {
        var employeeRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Admin",
            "Lekarz",
            "Rejestratorka"
        };

        var result = new HashSet<string>();
        var allUsers = _userManager.Users.ToList();

        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any(role => employeeRoles.Contains(role)))
            {
                continue;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var pesel = claims.FirstOrDefault(claim => claim.Type == "PatientPesel")?.Value;
            if (!string.IsNullOrWhiteSpace(pesel))
            {
                result.Add(pesel.Trim());
            }
        }

        return result;
    }
}
