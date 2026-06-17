using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IUserManagementService
{
    Task<List<UserRolesDto>> GetAllUsersWithRolesAsync();
    Task<List<EmployeeDto>> GetEmployeesAsync();
    Task<UserRolesDto?> GetUserRolesAsync(string id);
    Task<(bool Success, string ErrorMessage)> EditRolesAsync(string id, List<string> roleNames);
    Task<(bool Success, string ErrorMessage)> DeleteUserAsync(string id);
    Task<HashSet<string>> GetEmployeePatientPeselsAsync();
}
