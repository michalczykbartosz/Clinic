using System.ComponentModel.DataAnnotations;

namespace ClinicManager.DTOs;



public class RegisterDto
{
    [Required(ErrorMessage = "Imię jest wymagane.")]
    [StringLength(80, ErrorMessage = "Imię może mieć maksymalnie 80 znaków.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko jest wymagane.")]
    [StringLength(80, ErrorMessage = "Nazwisko może mieć maksymalnie 80 znaków.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage="Adres e-mail jest wymagany!")]
    [EmailAddress(ErrorMessage="Zły format adresu e-mail!")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage="Hasło jest wymagane!")]
    [DataType(DataType.Password)]
    [StringLength(100,ErrorMessage= "Hasło musi mieć co najmniej {2} znaków!",MinimumLength = 10)]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage="Potwierdź hasło!")]
    [DataType(DataType.Password)]
    [Compare("Password",ErrorMessage = "Hasła nie są takie same!")]
    public string PasswordConfirm { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "PESEL jest wymagany.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi mieć dokładnie 11 znaków.")]
    [RegularExpression("^[0-9]{11}$", ErrorMessage = "PESEL może zawierać wyłącznie cyfry!")]
    public string Pesel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numer ubezpieczenia jest wymagany.")]
    [StringLength(40, ErrorMessage = "Numer ubezpieczenia może mieć maksymalnie 40 znaków.")]
    public string InsuranceNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data urodzenia jest wymagana.")]
    [DataType(DataType.Date)]
    public DateOnly BirthDate { get; set; }

}
