using System.ComponentModel.DataAnnotations;
namespace ClinicManager.DTOs;

public class ReportCostDto
{
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public decimal OverallCost { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}