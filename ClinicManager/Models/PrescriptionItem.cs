namespace ClinicManager.Models;

public class PrescriptionItem
{
    public int PrescriptionItemId { get; set; }
    public int PrescriptionId { get; set; }
    public  Prescription Prescription { get; set; }
    public int MedicationId { get; set; }
    public Medication Medication { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; }
}