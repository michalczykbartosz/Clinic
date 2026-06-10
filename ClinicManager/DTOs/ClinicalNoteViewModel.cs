using ClinicManager.Models;
namespace ClinicManager.DTOs;

public class ClinicalNoteViewModel
{
    public int VisitId { get; set; }
    public ClinicalNote Note { get; set; } 
}