namespace ClinicManager.Models;

public class Prescription
{
    public int PrescriptionId { get; set; }
    public int VisitId { get; set; }
    public Visit Visit { get; set; }
    public List<PrescriptionItem> PrescriptionItems{ get; set; }
}