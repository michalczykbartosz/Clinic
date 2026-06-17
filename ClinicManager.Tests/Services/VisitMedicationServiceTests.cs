using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class VisitMedicationServiceTests
{
    [Test]
    public async Task BuildCreateModelAsync_WhenVisitDoesNotExist_ReturnsNull()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        var service = CreateService(dbContext);

        var model = await service.BuildCreateModelAsync(99, CancellationToken.None);

        Assert.That(model, Is.Null);
    }

    [Test]
    public async Task AddMedicationAsync_WhenVisitAndMedicationExist_CreatesPrescriptionItem()
    {
        await using var dbContext = await SeedVisitMedicationDataAsync();
        var service = CreateService(dbContext);

        var visitId = await service.AddMedicationAsync(
            new CreateVisitMedicationDto
            {
                VisitId = 1,
                MedicationId = 1,
                Dosage = " 1 tabletka rano ",
                Quantity = 2
            },
            CancellationToken.None);

        var item = await dbContext.PrescriptionItems.SingleAsync();
        Assert.That(visitId, Is.EqualTo(1));
        Assert.That(item.Description, Is.EqualTo("1 tabletka rano"));
        Assert.That(item.Quantity, Is.EqualTo(2));
    }

    [Test]
    public async Task GetForVisitAsync_ReturnsMedicationDetailsOrderedByName()
    {
        await using var dbContext = await SeedVisitMedicationDataAsync();
        dbContext.Medications.Add(TestData.Medication(2, "Acard", dose: "75mg"));
        dbContext.Prescriptions.Add(new ClinicManager.Models.Prescription
        {
            VisitId = 1,
            PrescriptionItems =
            [
                new ClinicManager.Models.PrescriptionItem
                {
                    MedicationId = 1,
                    Description = "Wieczorem",
                    Quantity = 1
                },
                new ClinicManager.Models.PrescriptionItem
                {
                    MedicationId = 2,
                    Description = "Rano",
                    Quantity = 1
                }
            ]
        });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var model = await service.GetForVisitAsync(1, CancellationToken.None);

        Assert.That(model, Is.Not.Null);
        Assert.That(model!.Medications.Select(medication => medication.MedicationName), Is.EqualTo(new[] { "Acard", "Apap" }));
    }

    private static VisitMedicationService CreateService(ClinicDbContext dbContext)
    {
        return new VisitMedicationService(
            dbContext,
            new PrescriptionItemMapper(),
            NullLogger<VisitMedicationService>.Instance);
    }

    private static async Task<TestClinicDbContext> SeedVisitMedicationDataAsync()
    {
        var dbContext = await TestDbContextFactory.CreateAsync();
        await TestData.SeedPeopleAsync(dbContext);

        dbContext.Visits.Add(TestData.Visit());
        dbContext.Medications.Add(TestData.Medication());
        await dbContext.SaveChangesAsync();

        return dbContext;
    }
}
