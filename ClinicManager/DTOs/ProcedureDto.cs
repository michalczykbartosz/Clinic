using System.ComponentModel.DataAnnotations;

namespace ClinicManager.DTOs;

public class ProcedureDto
{
    public int ProcedureId { get; set; }
    public int? VisitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public DateTime Date { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;
}

public class CreateProcedureDto
{
    public int VisitId { get; set; }

    [Display(Name = "Nazwa")]
    [Required(ErrorMessage = "Nazwa procedury jest wymagana.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Opis")]
    [Required(ErrorMessage = "Opis procedury jest wymagany.")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Koszt")]
    [Range(0.01, 999999.99, ErrorMessage = "Koszt musi być większy od 0.")]
    public decimal Cost { get; set; }
}
