using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManager.Models;

public class Procedure
{
    public int ProcedureId { get; set; }

    [NotMapped]
    public string Name { get; set; } = string.Empty;

    public int DoctorId { get; set; }
    public int MedicalRecordId { get; set; }

    [NotMapped]
    public int? VisitId { get; set; }

    public MedicalRecord MedicalRecord { get; set; }
    public Doctor Doctor { get; set; }

    [NotMapped]
    public Visit? Visit { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public DateTime Date { get; set; }
}
