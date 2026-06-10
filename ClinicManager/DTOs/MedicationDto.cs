using System.ComponentModel.DataAnnotations;
namespace ClinicManager.DTOs;

public class MedicationDto
{
    public int MedicationId { get; set; }
    [Required(ErrorMessage = "Nazwa jest wymagana!")]
    [MaxLength(300,ErrorMessage = "Za dużo znaków!")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Nazwa producenta jest wymagana!")]
    [MaxLength(300,ErrorMessage = "Za dużo znaków!")]
    public string Manufacturer { get; set; } = string.Empty;
    [Required(ErrorMessage = "Dawka jest wymagana!")]
    [MaxLength(20,ErrorMessage = "Za dużo znaków!")]
    public string Dose { get; set; } = string.Empty;
}
