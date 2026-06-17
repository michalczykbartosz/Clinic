using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class MedicationServiceTests
{
    [Test]
    public async Task GetAllAsync_ReturnsMedicationsOrderedByName()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        dbContext.Medications.AddRange(
            TestData.Medication(1, "Zyrtec"),
            TestData.Medication(2, "Apap"));
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var medications = await service.GetAllAsync(CancellationToken.None);

        Assert.That(medications.Select(medication => medication.Name), Is.EqualTo(new[] { "Apap", "Zyrtec" }));
    }

    [Test]
    public async Task AddMedicationAsync_PersistsNewMedication()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        var service = CreateService(dbContext);

        var result = await service.AddMedicationAsync(new MedicationDto
        {
            Name = "Ibuprom",
            Manufacturer = "USP",
            Dose = "400mg"
        });

        Assert.That(result.success, Is.True);
        Assert.That(await dbContext.Medications.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateMedicationAsync_ChangesExistingMedication()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        dbContext.Medications.Add(TestData.Medication());
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        var service = CreateService(dbContext);

        var result = await service.UpdateMedicationAsync(new MedicationDto
        {
            MedicationId = 1,
            Name = "Apap Extra",
            Manufacturer = "USP",
            Dose = "1000mg"
        });

        var medication = await dbContext.Medications.FindAsync(1);
        Assert.That(result.success, Is.True);
        Assert.That(medication!.Name, Is.EqualTo("Apap Extra"));
        Assert.That(medication.Dose, Is.EqualTo("1000mg"));
    }

    private static MedicationService CreateService(ClinicDbContext dbContext)
    {
        return new MedicationService(
            dbContext,
            new MedicationMapper(),
            NullLogger<MedicationService>.Instance);
    }
}
