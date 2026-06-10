using ClinicManager.DTOs;
using ClinicManager.Models;
using ClinicManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ClinicManager.Controllers;

[Authorize(Roles="Lekarz")]
public class ClinicalNoteController : Controller
{
    private readonly ClinicDbContext _context;
    
    public ClinicalNoteController(ClinicDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetNote(int visitId)
    {
        ClinicalNoteViewModel model = new ClinicalNoteViewModel();
        ClinicalNote note = await _context.ClinicalNotes.Where(x => x.VisitId == visitId).FirstOrDefaultAsync();
        model.VisitId = visitId;
        model.Note = note;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrUpdateNote(int visitId,string newNote)
    {
        ClinicalNote note = await _context.ClinicalNotes.FirstOrDefaultAsync(x=>x.VisitId == visitId);
        if (note is null)
        {
            ClinicalNote newClinicalNote = new ClinicalNote();
            newClinicalNote.VisitId = visitId;
            newClinicalNote.Note = newNote;
            _context.ClinicalNotes.Add(newClinicalNote);
            await _context.SaveChangesAsync();
        }
        else
        {
            note.Note = newNote;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("GetNote", new { visitId });


    }
}