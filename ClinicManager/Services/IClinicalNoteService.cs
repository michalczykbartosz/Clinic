using ClinicManager.Models;
using ClinicManager.DTOs;
namespace ClinicManager.Services;

public interface IClinicalNoteService
{
    Task<(bool success, ClinicalNoteDto? note, string errorMessage)> GetNoteAsync(int visitId);
    Task<(bool success, string errorMessage)> CreateOrUpdateNoteAsync(ClinicalNoteDto newNote);
}