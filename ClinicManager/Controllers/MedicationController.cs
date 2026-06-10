using ClinicManager.Services;
using Microsoft.AspNetCore.Mvc;
using ClinicManager.DTOs;
using ClinicManager.Models;

namespace ClinicManager.Controllers;

public class MedicationController : Controller
{
    private readonly IMedicationService _medicationService;

    public MedicationController(IMedicationService medicationService)
    {
        _medicationService = medicationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        IReadOnlyList<MedicationDto> allMedicationsDto = await _medicationService.GetAllAsync();
        return View(allMedicationsDto);

    }

    [HttpGet]
    public async Task<IActionResult> Edit(int medicationId)
    {
        MedicationDto wantedMedicationDto = await _medicationService.GetByIdAsync(medicationId);
        if (wantedMedicationDto is null) return RedirectToAction("Index", "Medication");
        return View(wantedMedicationDto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(MedicationDto newMedicationdto)
    {
        if (!ModelState.IsValid) return View(newMedicationdto);
        var (success, error) = await _medicationService.UpdateMedicationAsync(newMedicationdto);
        if (success) return RedirectToAction("Index", "Medication");
        ModelState.AddModelError(string.Empty,error);
        return View(newMedicationdto);
    }

    [HttpPost]
    public async Task<IActionResult> Save(MedicationDto newMedicationDto)
    {
        if (!ModelState.IsValid) return View(newMedicationDto);
        var (success,error) = await _medicationService.AddMedicationAsync(newMedicationDto);
        if(success) return RedirectToAction("Index", "Medication");
        ModelState.AddModelError(string.Empty,error);
        return View(newMedicationDto);
    }

    [HttpGet]
    public IActionResult Save()
    {
        return View();
    }
    
    
}