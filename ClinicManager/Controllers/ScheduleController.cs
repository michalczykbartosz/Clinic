using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
public class ScheduleController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IVisitService _visitService;

    public ScheduleController(IDoctorService doctorService, IVisitService visitService)
    {
        _doctorService = doctorService;
        _visitService = visitService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? doctorId, DateOnly? date, CancellationToken cancellationToken)
    {
        var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var doctors = await _doctorService.GetAllAsync(cancellationToken);
        var selectedDoctorId = doctorId ?? doctors.FirstOrDefault()?.DoctorId;
        var selectedDoctor = selectedDoctorId is null
            ? null
            : doctors.FirstOrDefault(doctor => doctor.DoctorId == selectedDoctorId.Value);

        var visits = selectedDoctorId is null
            ? Array.Empty<ClinicManager.DTOs.VisitListItemDto>()
            : await _visitService.GetDoctorScheduleAsync(selectedDoctorId.Value, scheduleDate, cancellationToken);

        var model = new ScheduleViewModel
        {
            DoctorId = selectedDoctorId,
            Date = scheduleDate,
            SelectedDoctor = selectedDoctor,
            Visits = visits,
            Doctors = doctors
                .Select(doctor => new SelectListItem
                {
                    Value = doctor.DoctorId.ToString(),
                    Text = $"{doctor.LastName} {doctor.FirstName} - {doctor.Specialization}",
                    Selected = selectedDoctorId == doctor.DoctorId
                })
                .ToList()
        };

        return View(model);
    }
}
