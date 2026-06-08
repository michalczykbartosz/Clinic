namespace ClinicManager.Models;

public class Procedure
{
    public int ProcedureId { get; set; }
    public int DoctorId { get; set; }
    public int MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; }
    public Doctor Doctor { get; set; }
    public string Description { get; set; }
    public decimal Cost { get; set; }
    public DateTime Date { get; set; }

}