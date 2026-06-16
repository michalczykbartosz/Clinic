using ClinicManager.Models;
using Microsoft.AspNetCore.Identity;

namespace ClinicManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class ClinicDbContext : IdentityDbContext
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
    public DbSet<PatientDocument> PatientDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 
        modelBuilder.Entity<Procedure>().Property(x => x.Cost).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<PatientDocument>()
            .HasOne(document => document.Patient)
            .WithMany(patient => patient.Documents)
            .HasForeignKey(document => document.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Doctor>().HasData(
            new Doctor { DoctorId = 1, FirstName = "Adam", LastName = "Wiśniewski", PESEL = "75081911223", BirthDate = new DateOnly(1975, 8, 19), PwzNumber = "1234567", Specialization = "Kardiolog" },
            new Doctor { DoctorId = 2, FirstName = "Ewa", LastName = "Kowalczyk", PESEL = "82031509876", BirthDate = new DateOnly(1982, 3, 15), PwzNumber = "7654321", Specialization = "Neurolog" });

        modelBuilder.Entity<Patient>().HasData(
            new Patient { PatientId = 1, FirstName = "Jan", LastName = "Nowak", PESEL = "90051401234", InsuranceNumber = "NFZ-998877", BirthDate = new DateOnly(1990, 5, 14) });

        modelBuilder.Entity<Medication>().HasData(
            new Medication { MedicationId = 1, Name = "Ibuprom Max", Manufacturer = "US Pharmacia", Dose = "400mg" });
        
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "Pacjent", NormalizedName = "PACJENT",ConcurrencyStamp = "test"});
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "2", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "test" });
        modelBuilder.Entity<Visit>().HasData(
            new Visit { VisitId = 1, VisitStatus = VisitState.Planned, PatientId = 1, DoctorId = 1, VisitDateTime = new DateTime(2026, 6, 15, 14, 0, 0) },
            new Visit { VisitId = 2, VisitStatus = VisitState.InProgress, PatientId = 1, DoctorId = 2, VisitDateTime = new DateTime(2026, 6, 10, 10, 30, 0) });
    }
    
}
