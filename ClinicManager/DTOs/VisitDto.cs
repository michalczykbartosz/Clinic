using System.ComponentModel.DataAnnotations;
using ClinicManager.Models;

namespace ClinicManager.DTOs;

public class VisitDto
{
    public int VisitId { get; set; }
    public VisitState VisitStatus { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime VisitDateTime { get; set; }
}

public class ActiveVisitDto
{
    public int VisitId { get; set; }
    public VisitState VisitStatus { get; set; }
    public DateTime VisitDateTime { get; set; }
    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public string PatientPESEL { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
}

public class PatientVisitDto
{
    public int VisitId { get; set; }
    public VisitState VisitStatus { get; set; }
    public DateTime VisitDateTime { get; set; }
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
}

public class CreateVisitDto
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public int DoctorId { get; set; }

    [Required]
    public DateTime VisitDateTime { get; set; }
}

public class UpdateVisitStatusDto
{
    [Required]
    public VisitState VisitStatus { get; set; }
}
