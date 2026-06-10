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
    public ClinicalNoteController(IClinicalNoteService noteService)
    {
        _noteService = noteService;
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
    public async Task<IActionResult> SaveNote(ClinicalNoteDto dtoNote)
    {
        if (!ModelState.IsValid) return View(dtoNote);
        var (success, error) = await _noteService.CreateOrUpdateNoteAsync(dtoNote);
        if (success is true) return RedirectToAction("Index","Home");
        ModelState.AddModelError(string.Empty, error);
        return View(dtoNote);

    }
}