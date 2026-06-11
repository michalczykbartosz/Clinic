using ClinicManager.DTOs;

namespace ClinicManager.ViewModels;

public class PatientDetailsViewModel
{
    public PatientDto Patient { get; set; } = new();
    public IReadOnlyList<PatientVisitDto> Visits { get; set; } = Array.Empty<PatientVisitDto>();
}
