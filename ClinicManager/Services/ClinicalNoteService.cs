using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class ClinicalNoteService :IClinicalNoteService
{
    private readonly ClinicDbContext _context;
    private readonly ClinicalNoteMapper _mapper;
    private readonly ILogger<ClinicalNoteService> _logger;

    public ClinicalNoteService(ClinicDbContext context, ClinicalNoteMapper mapper, ILogger<ClinicalNoteService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<(bool success, ClinicalNoteDto? note, string errorMessage)> GetNoteAsync(int visitId)
    {
        ClinicalNote? note = await _context.ClinicalNotes.FirstOrDefaultAsync(x => x.VisitId == visitId);
        if (note is null)
        {
            _logger.LogInformation("Nie znaleziono notatki klinicznej dla wizyty {VisitId}", visitId);
            return (false, null, "Nie znaleziono notatki!");
        }

        ClinicalNoteDto dtoNote = _mapper.ToDto(note);
        return (true, dtoNote, "");
    }
    
    public async Task<(bool success,string errorMessage)> CreateOrUpdateNoteAsync(ClinicalNoteDto newNote)
    {
        ClinicalNote? note = await _context.ClinicalNotes.FirstOrDefaultAsync(x => x.VisitId == newNote.VisitId);
        if (note is null)
        {
            note = _mapper.ToEntity(newNote);
            await _context.ClinicalNotes.AddAsync(note);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Utworzono notatkę kliniczną dla wizyty {VisitId}", newNote.VisitId);
            return (true, "");
        }
            note.Note = newNote.Note;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Zaktualizowano notatkę kliniczną dla wizyty {VisitId}", newNote.VisitId);
            return (true, "");
            
    }
}
