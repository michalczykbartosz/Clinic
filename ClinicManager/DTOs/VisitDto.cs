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

public class VisitListItemDto
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
    [Display(Name = "Pacjent")]
    [Range(1, int.MaxValue, ErrorMessage = "Wybierz pacjenta.")]
    public int PatientId { get; set; }

    [Display(Name = "Lekarz")]
    [Range(1, int.MaxValue, ErrorMessage = "Wybierz lekarza.")]
    public int DoctorId { get; set; }

    [Display(Name = "Data i godzina wizyty")]
    [Required(ErrorMessage = "Data i godzina wizyty jest wymagana.")]
    public DateTime VisitDateTime { get; set; }
}

public class UpdateVisitStatusDto
{
    [Display(Name = "Status wizyty")]
    [Required(ErrorMessage = "Status wizyty jest wymagany.")]
    [EnumDataType(typeof(VisitState), ErrorMessage = "Wybrano nieprawidłowy status wizyty.")]
    public VisitState VisitStatus { get; set; }
}
