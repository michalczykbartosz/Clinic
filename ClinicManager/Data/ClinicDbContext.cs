using ClinicManager.Models;

namespace ClinicManager.Data;
using Microsoft.EntityFrameworkCore;

public class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
    {
        
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<ClinicalNote> ClinicalNotes { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public  DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
    public DbSet<Procedure> Procedures { get; set; }
    public DbSet<Visit> Visits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 
        modelBuilder.Entity<Procedure>().Property(x => x.Cost).HasColumnType("decimal(18,2)");
        
        modelBuilder.Entity<Doctor>().HasData(
            new Doctor { DoctorId = 1, FirstName = "Adam", LastName = "Wiśniewski", PESEL = "75081911223", BirthDate = new DateOnly(1975, 8, 19), PwzNumber = "1234567", Specialization = "Kardiolog" },
            new Doctor { DoctorId = 2, FirstName = "Ewa", LastName = "Kowalczyk", PESEL = "82031509876", BirthDate = new DateOnly(1982, 3, 15), PwzNumber = "7654321", Specialization = "Neurolog" });

        modelBuilder.Entity<Patient>().HasData(
            new Patient { PatientId = 1, FirstName = "Jan", LastName = "Nowak", PESEL = "90051401234", InsuranceNumber = "NFZ-998877", BirthDate = new DateOnly(1990, 5, 14) });

        modelBuilder.Entity<Medication>().HasData(
            new Medication { MedicationId = 1, Name = "Ibuprom Max", Manufacturer = "US Pharmacia", Dose = "400mg" });
    }
}