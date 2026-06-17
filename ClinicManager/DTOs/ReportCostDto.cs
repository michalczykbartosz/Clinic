namespace ClinicManager.DTOs;

public enum ReportCostScope
{
    Procedures,
    Visits,
    VisitsAndProcedures
}

public class ReportCostDto
{
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public ReportCostScope Scope { get; set; }
    public decimal OverallCost { get; set; }
    public decimal ProcedureCost { get; set; }
    public decimal VisitCost { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public string ScopeLabel => Scope switch
    {
        ReportCostScope.Procedures => "Koszty procedur",
        ReportCostScope.Visits => "Koszty wizyt",
        ReportCostScope.VisitsAndProcedures => "Wizyty + procedury",
        _ => "Raport kosztów"
    };
}
