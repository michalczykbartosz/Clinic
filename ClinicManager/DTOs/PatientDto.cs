using System.ComponentModel.DataAnnotations;

namespace ClinicManager.DTOs;

public class PatientDto
{
    public int PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PESEL { get; set; } = string.Empty;
    public string InsuranceNumber { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
}

public class UpsertPatientDto
{
    [Required]
    [StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi mieć dokładnie 11 cyfr!")]
    [RegularExpression("^[0-9]{11}$", ErrorMessage = "PESEL może zawierać wyłącznie cyfry!")]
    public string PESEL { get; set; } = string.Empty;

    [Required]
    [StringLength(40)]
    public string InsuranceNumber { get; set; } = string.Empty;

    [Required]
    public DateOnly BirthDate { get; set; }
}
