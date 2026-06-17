using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class ProcedureServiceTests
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
    public async Task CreateAsync_WhenVisitExists_CreatesMedicalRecordAndProcedure()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        await TestData.SeedPeopleAsync(dbContext);
        dbContext.Visits.Add(TestData.Visit());
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var visitId = await service.CreateAsync(
            new CreateProcedureDto
            {
                VisitId = 1,
                Name = "Skaling",
                Description = "Opis zabiegu",
                Cost = 150m
            },
            CancellationToken.None);

        Assert.That(visitId, Is.EqualTo(1));
        Assert.That(await dbContext.MedicalRecords.CountAsync(), Is.EqualTo(1));
        Assert.That(await dbContext.Procedures.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetForVisitAsync_ReturnsProceduresForVisitPatient()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        await TestData.SeedPeopleAsync(dbContext);
        dbContext.Visits.Add(TestData.Visit());
        dbContext.MedicalRecords.Add(TestData.MedicalRecord());
        dbContext.Procedures.Add(TestData.Procedure(description: "Nazwa: Wypelnienie\r\n\r\nZabieg kontrolny"));
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var model = await service.GetForVisitAsync(1, CancellationToken.None);

        Assert.That(model, Is.Not.Null);
        Assert.That(model!.Procedures, Has.Count.EqualTo(1));
        Assert.That(model.Procedures[0].Name, Is.EqualTo("Wypelnienie"));
        Assert.That(model.Procedures[0].Description, Is.EqualTo("Zabieg kontrolny"));
    }

    private static ProcedureService CreateService(ClinicDbContext dbContext)
    {
        return new ProcedureService(dbContext, NullLogger<ProcedureService>.Instance);
    }
}
