namespace ClinicManager.Models;
using System.ComponentModel.DataAnnotations;

public class Patient
{
    public int PatientId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    [StringLength(11,MinimumLength = 11, ErrorMessage = "PESEL musi mieć dokładnie 11 cyfr!")]
    [RegularExpression("^[0-9]{11}$",ErrorMessage = "PESEL może zawierać wyłącznie cyfry!")]
    public string PESEL { get; set; }
    public string InsuranceNumber { get; set; }
    public DateOnly BirthDate { get; set; }
    public List<Visit> VisitList { get; set; }
}