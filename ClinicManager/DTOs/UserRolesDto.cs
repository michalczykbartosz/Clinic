using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.DTOs;

public class UserRolesDto
{
    

    public string Email { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
    public List<string> AllRoles { get; set; } = new List<string>();



}
