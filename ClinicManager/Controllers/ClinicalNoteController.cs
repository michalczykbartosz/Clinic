using ClinicManager.DTOs;
using ClinicManager.Models;
using ClinicManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ClinicManager.Controllers;

[Authorize(Roles="Lekarz")]
public class ClinicalNoteController : Controller
{
    private readonly IClinicalNoteService _noteService;
    private readonly ILogger<ClinicalNoteController> _logger;

    public ClinicalNoteController(IClinicalNoteService noteService, ILogger<ClinicalNoteController> logger)
    {
        _noteService = noteService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetNote(int visitId)
    {
        var (success, dto, errorMessage) = await _noteService.GetNoteAsync(visitId);
        if (dto is null)
        {
            ClinicalNoteDto newNote = new ClinicalNoteDto {VisitId = visitId};
            return View(newNote);
        }

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNote(ClinicalNoteDto dtoNote)
    {
        if (!ModelState.IsValid) return View("GetNote", dtoNote);
        var (success, error) = await _noteService.CreateOrUpdateNoteAsync(dtoNote);
        if (success is true)
        {
            _logger.LogInformation("Zapisano notatkę kliniczną dla wizyty {VisitId}", dtoNote.VisitId);
            return RedirectToAction("Index","Visits");
        }

        _logger.LogWarning("Nie udało się zapisać notatki klinicznej dla wizyty {VisitId}: {Error}", dtoNote.VisitId, error);
        ModelState.AddModelError(string.Empty, error);
        return View("GetNote", dtoNote);

    }
}
