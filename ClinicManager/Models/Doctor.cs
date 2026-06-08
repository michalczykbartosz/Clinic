namespace ClinicManager.Models;

public class Doctor
{
    public int DoctorId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PESEL { get; set; }
    public DateOnly BirthDate { get; set; }
    public string PwzNumber { get; set; }
    public string Specialization { get; set; }
    public List<Procedure> Procedures { get; set; }
    public List<Visit> Visits { get; set; }
    
}