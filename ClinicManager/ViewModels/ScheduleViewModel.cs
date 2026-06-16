using ClinicManager.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicManager.ViewModels;

public class ScheduleViewModel
{
    public int? DoctorId { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public List<SelectListItem> Doctors { get; set; } = [];
    public IReadOnlyList<VisitListItemDto> Visits { get; set; } = Array.Empty<VisitListItemDto>();
    public DoctorDto? SelectedDoctor { get; set; }
}
