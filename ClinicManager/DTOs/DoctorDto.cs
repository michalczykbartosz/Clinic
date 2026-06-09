namespace ClinicManager.DTOs;

public class DoctorDto
{
    public int DoctorId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PESEL { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string PwzNumber { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
}
