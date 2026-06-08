namespace ClinicManager.Models;

public class MedicalRecord
{
    public int MedicalRecordId { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; }
    public List<Procedure> Procedures { get; set; }
    public string Description { get; set; }
    public DateTime DescriptionModifyTime { get; set; }

}