using ClinicManager.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Tests;

internal static class TestDbContextFactory
{
    public static async Task<TestClinicDbContext> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new TestClinicDbContext(options, connection);
        await dbContext.Database.EnsureCreatedAsync();
        await ClearSeedDataAsync(dbContext);

        return dbContext;
    }

    private static async Task ClearSeedDataAsync(ClinicDbContext dbContext)
    {
        dbContext.PrescriptionItems.RemoveRange(dbContext.PrescriptionItems);
        dbContext.Prescriptions.RemoveRange(dbContext.Prescriptions);
        dbContext.Procedures.RemoveRange(dbContext.Procedures);
        dbContext.ClinicalNotes.RemoveRange(dbContext.ClinicalNotes);
        dbContext.MedicalRecords.RemoveRange(dbContext.MedicalRecords);
        dbContext.PatientDocuments.RemoveRange(dbContext.PatientDocuments);
        dbContext.Visits.RemoveRange(dbContext.Visits);
        dbContext.Patients.RemoveRange(dbContext.Patients);
        dbContext.Doctors.RemoveRange(dbContext.Doctors);
        dbContext.Medications.RemoveRange(dbContext.Medications);

        await dbContext.SaveChangesAsync();
    }
}
