using System.ComponentModel.DataAnnotations;

namespace ClinicManager.DTOs;

public class PatientDto
{
    public int PatientId { get; set; }

    [Display(Name = "Imię")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "Nazwisko")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "PESEL")]
    public string PESEL { get; set; } = string.Empty;

    [Display(Name = "Numer ubezpieczenia")]
    public string InsuranceNumber { get; set; } = string.Empty;

    [Display(Name = "Data urodzenia")]
    [DataType(DataType.Date)]
    public DateOnly BirthDate { get; set; }
}

public class UpsertPatientDto
{
    [Display(Name = "Imię")]
    [Required(ErrorMessage = "Imię jest wymagane.")]
    [StringLength(80, ErrorMessage = "Imię może mieć maksymalnie 80 znaków.")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "Nazwisko")]
    [Required(ErrorMessage = "Nazwisko jest wymagane.")]
    [StringLength(80, ErrorMessage = "Nazwisko może mieć maksymalnie 80 znaków.")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "PESEL")]
    [Required(ErrorMessage = "PESEL jest wymagany.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi mieć dokładnie 11 cyfr!")]
    [RegularExpression("^[0-9]{11}$", ErrorMessage = "PESEL może zawierać wyłącznie cyfry!")]
    public string PESEL { get; set; } = string.Empty;

    [Display(Name = "Numer ubezpieczenia")]
    [Required(ErrorMessage = "Numer ubezpieczenia jest wymagany.")]
    [StringLength(40, ErrorMessage = "Numer ubezpieczenia może mieć maksymalnie 40 znaków.")]
    public string InsuranceNumber { get; set; } = string.Empty;

    [Display(Name = "Data urodzenia")]
    [Required(ErrorMessage = "Data urodzenia jest wymagana.")]
    [DataType(DataType.Date)]
    public DateOnly BirthDate { get; set; }
}
