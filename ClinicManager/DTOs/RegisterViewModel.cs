using System.ComponentModel.DataAnnotations;

namespace ClinicManager.DTOs;



public class RegisterViewModel
{
    [Required(ErrorMessage="Adres e-mail jest wymagany!")]
    [EmailAddress(ErrorMessage="Zły format adresu e-mail!")]
    public string Email { get; set; }
    
    [Required(ErrorMessage="Hasło jest wymagane!")]
    [DataType(DataType.Password)]
    [StringLength(100,ErrorMessage= "Hasło musi mieć co najmniej {2} znaków!",MinimumLength = 10)]
    public string Password { get; set; }
    
    [Required(ErrorMessage="Potwierdź hasło!")]
    [DataType(DataType.Password)]
    [Compare("Password",ErrorMessage = "Hasła nie są takie same!")]
    public string PasswordConfirm { get; set; }
    
    [Required(ErrorMessage = "PESEL jest wymagany.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi mieć dokładnie 11 znaków.")]
    public string Pesel { get; set; }

}