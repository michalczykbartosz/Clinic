using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.DTOs;

public class UserRolesViewModel
{
    

    public string Email { get; set; }

    public string Id { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public List<string> AllRoles { get; set; } = new List<string>();



}