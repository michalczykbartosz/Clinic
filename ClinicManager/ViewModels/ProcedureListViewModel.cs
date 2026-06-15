using ClinicManager.DTOs;

namespace ClinicManager.ViewModels;

public class ProcedureListViewModel
{
    public int VisitId { get; set; }
    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public DateTime VisitDateTime { get; set; }
    public IReadOnlyList<ProcedureDto> Procedures { get; set; } = Array.Empty<ProcedureDto>();
}
