using ClinicManager.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicManager.ViewModels;

public class VisitMedicationsViewModel
{
    public int VisitId { get; set; }
    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public string DoctorFullName { get; set; } = string.Empty;
    public DateTime VisitDateTime { get; set; }
    public IReadOnlyList<VisitMedicationDto> Medications { get; set; } = [];
}

public class CreateVisitMedicationViewModel
{
    public CreateVisitMedicationDto Medication { get; set; } = new();
    public IReadOnlyList<SelectListItem> AvailableMedications { get; set; } = [];
}
