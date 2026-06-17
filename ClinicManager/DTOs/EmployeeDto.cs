namespace ClinicManager.DTOs;

public class EmployeeDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Pesel { get; set; } = string.Empty;
    public int? DoctorId { get; set; }
    public List<string> Roles { get; set; } = [];

    public string FullName => string.IsNullOrWhiteSpace($"{FirstName} {LastName}".Trim())
        ? "Brak danych"
        : $"{FirstName} {LastName}".Trim();
}
