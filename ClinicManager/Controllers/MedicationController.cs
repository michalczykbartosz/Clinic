using ClinicManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManager.DTOs;
using ClinicManager.Models;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Lekarz")]
public class MedicationController : Controller
{
    private readonly IMedicationService _medicationService;
    private readonly ILogger<MedicationController> _logger;

    public MedicationController(IMedicationService medicationService, ILogger<MedicationController> logger)
    {
        _medicationService = medicationService;
        _logger = logger;
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
        MedicationDto? wantedMedicationDto = await _medicationService.GetByIdAsync(medicationId);
        if (wantedMedicationDto is null)
        {
            _logger.LogWarning("Nie znaleziono leku {MedicationId} do edycji.", medicationId);
            return RedirectToAction("Index", "Medication");
        }
        return View(wantedMedicationDto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(MedicationDto newMedicationdto)
    {
        if (!ModelState.IsValid) return View(newMedicationdto);
        var (success, error) = await _medicationService.UpdateMedicationAsync(newMedicationdto);
        if (success)
        {
            _logger.LogInformation("Zaktualizowano lek {MedicationId}", newMedicationdto.MedicationId);
            return RedirectToAction("Index", "Medication");
        }

        _logger.LogWarning("Nie udało się zaktualizować leku {MedicationId}: {Error}", newMedicationdto.MedicationId, error);
        ModelState.AddModelError(string.Empty,error);
        return View(newMedicationdto);
    }

    [HttpPost]
    public async Task<IActionResult> Save(MedicationDto newMedicationDto)
    {
        if (!ModelState.IsValid) return View(newMedicationDto);
        var (success,error) = await _medicationService.AddMedicationAsync(newMedicationDto);
        if(success)
        {
            _logger.LogInformation("Dodano lek {MedicationName}", newMedicationDto.Name);
            return RedirectToAction("Index", "Medication");
        }

        _logger.LogWarning("Nie udało się dodać leku {MedicationName}: {Error}", newMedicationDto.Name, error);
        ModelState.AddModelError(string.Empty,error);
        return View(newMedicationDto);
    }

    [HttpGet]
    public IActionResult Save()
    {
        return View();
    }
    
    
}
