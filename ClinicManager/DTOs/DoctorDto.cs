namespace ClinicManager.DTOs;

using System.ComponentModel.DataAnnotations;

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

public class UpdateDoctorProfileDto
{
    public int DoctorId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PESEL { get; set; } = string.Empty;

    [Display(Name = "Numer PWZ")]
    [StringLength(20, ErrorMessage = "Numer PWZ może mieć maksymalnie 20 znaków.")]
    public string PwzNumber { get; set; } = string.Empty;

    [Display(Name = "Specjalizacja")]
    [Required(ErrorMessage = "Specjalizacja jest wymagana.")]
    [StringLength(80, ErrorMessage = "Specjalizacja może mieć maksymalnie 80 znaków.")]
    public string Specialization { get; set; } = string.Empty;
}
