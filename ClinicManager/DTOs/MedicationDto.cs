namespace ClinicManager.DTOs;

public class MedicationDto
{
    public int MedicationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
}
