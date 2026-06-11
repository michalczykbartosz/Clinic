using ClinicManager.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicManager.ViewModels;

public class CreateVisitViewModel
{
    public CreateVisitDto Visit { get; set; } = new()
    {
        VisitDateTime = DateTime.Today.AddDays(1).AddHours(8)
    };

    public IReadOnlyList<SelectListItem> Patients { get; set; } = [];
    public IReadOnlyList<SelectListItem> Doctors { get; set; } = [];
}
