namespace ClinicManager.Models;

public enum VisitState
{
        Planned,
        InProgress,
        Finished,
        Canceled
}
public class Visit
{
    public int VisitId { get; set; }
    public VisitState VisitStatus { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime VisitDateTime { get; set; }
    public Doctor Doctor { get; set; }
    public Patient Patient { get; set; }
    
    
}