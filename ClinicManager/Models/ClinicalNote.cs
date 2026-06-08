namespace ClinicManager.Models;

public class ClinicalNote
{
    public int ClinicalNoteId { get; set; }
    public string Note { get; set; }
    public int VisitId { get; set; }
    public Visit Visit { get; set; }
    public DateTime CreatedAt { get; set; }
}