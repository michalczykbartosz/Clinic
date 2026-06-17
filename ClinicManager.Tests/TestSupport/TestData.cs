using ClinicManager.Data;
using ClinicManager.Models;

namespace ClinicManager.Tests;

internal static class TestData
{
    public static Patient Patient(
        int id = 1,
        string firstName = "Jan",
        string lastName = "Nowak",
        string pesel = "90051401234")
    {
        return new Patient
        {
            PatientId = id,
            FirstName = firstName,
            LastName = lastName,
            PESEL = pesel,
            InsuranceNumber = $"NFZ-{id}",
            BirthDate = new DateOnly(1990, 5, 14),
            VisitList = []
        };
    }

    public static Doctor Doctor(
        int id = 1,
        string firstName = "Adam",
        string lastName = "Wisniewski")
    {
        return new Doctor
        {
            DoctorId = id,
            FirstName = firstName,
            LastName = lastName,
            PESEL = $"7508191122{id}",
            BirthDate = new DateOnly(1975, 8, 19),
            PwzNumber = $"PWZ-{id}",
            Specialization = "Stomatolog",
            Procedures = [],
            Visits = []
        };
    }

    public static Visit Visit(
        int id = 1,
        int patientId = 1,
        int doctorId = 1,
        VisitState status = VisitState.Planned,
        DateTime? dateTime = null,
        decimal cost = 200m,
        bool isPaid = false)
    {
        return new Visit
        {
            VisitId = id,
            PatientId = patientId,
            DoctorId = doctorId,
            VisitStatus = status,
            VisitDateTime = dateTime ?? new DateTime(2026, 6, 18, 9, 0, 0),
            Cost = cost,
            IsPaid = isPaid
        };
    }

    public static Medication Medication(
        int id = 1,
        string name = "Apap",
        string manufacturer = "US Pharmacia",
        string dose = "500mg")
    {
        return new Medication
        {
            MedicationId = id,
            Name = name,
            Manufacturer = manufacturer,
            Dose = dose
        };
    }

    public static MedicalRecord MedicalRecord(int id = 1, int patientId = 1)
    {
        return new MedicalRecord
        {
            MedicalRecordId = id,
            PatientId = patientId,
            Description = string.Empty,
            DescriptionModifyTime = new DateTime(2026, 6, 1, 8, 0, 0),
            Procedures = []
        };
    }

    public static Procedure Procedure(
        int id = 1,
        int medicalRecordId = 1,
        int doctorId = 1,
        string description = "Nazwa: Skaling\r\n\r\nOpis zabiegu",
        decimal cost = 100m,
        DateTime? date = null)
    {
        return new Procedure
        {
            ProcedureId = id,
            MedicalRecordId = medicalRecordId,
            DoctorId = doctorId,
            Description = description,
            Cost = cost,
            Date = date ?? new DateTime(2026, 6, 18, 10, 0, 0)
        };
    }

    public static async Task SeedPeopleAsync(ClinicDbContext dbContext)
    {
        dbContext.Patients.AddRange(
            Patient(),
            Patient(2, "Anna", "Kowalska", "91020312345"));

        dbContext.Doctors.AddRange(
            Doctor(),
            Doctor(2, "Ewa", "Adamska"));

        await dbContext.SaveChangesAsync();
    }
}
