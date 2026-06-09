using System.ComponentModel.DataAnnotations;

namespace ClinicManager.DTOs;

public class LoginViewModel
{
    [Required(ErrorMessage="Email jest wymagany!")]
    [EmailAddress(ErrorMessage = "Zły format adresu e-mail!")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Podaj hasło!")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}