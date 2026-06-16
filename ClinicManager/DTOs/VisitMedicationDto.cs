using System.ComponentModel.DataAnnotations;

namespace ClinicManager.DTOs;

public class VisitMedicationDto
{
    public int PrescriptionItemId { get; set; }
    public int MedicationId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class CreateVisitMedicationDto
{
    public int VisitId { get; set; }

    [Display(Name = "Lek")]
    [Range(1, int.MaxValue, ErrorMessage = "Wybierz lek z katalogu.")]
    public int MedicationId { get; set; }

    [Display(Name = "Dawkowanie")]
    [Required(ErrorMessage = "Dawkowanie jest wymagane.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Dawkowanie jest wymagane.")]
    [StringLength(500, ErrorMessage = "Dawkowanie może mieć maksymalnie 500 znaków.")]
    public string Dosage { get; set; } = string.Empty;

    [Display(Name = "Ilość")]
    [Range(1, int.MaxValue, ErrorMessage = "Ilość musi być większa od 0.")]
    public int Quantity { get; set; } = 1;
}
