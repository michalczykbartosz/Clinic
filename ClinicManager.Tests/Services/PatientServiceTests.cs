using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class PatientServiceTests
{
    [Test]
    public async Task SearchAsync_WhenQueryMatchesLastName_ReturnsOnlyMatchingPatients()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        await TestData.SeedPeopleAsync(dbContext);
        var service = CreateService(dbContext);

        var patients = await service.SearchAsync("Kowal", CancellationToken.None);

        Assert.That(patients, Has.Count.EqualTo(1));
        Assert.That(patients[0].PESEL, Is.EqualTo("91020312345"));
    }

    [Test]
    public async Task CreateAsync_WhenPeselAlreadyExists_ThrowsInvalidOperationException()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        dbContext.Patients.Add(TestData.Patient());
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var dto = NewPatientDto(pesel: "90051401234");

        Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto, CancellationToken.None));
    }

    [Test]
    public async Task UpdateRecordAsync_WhenPatientExists_TrimsAndSavesRecordData()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        dbContext.Patients.Add(TestData.Patient());
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var updated = await service.UpdateRecordAsync(
            1,
            new UpdatePatientRecordDto
            {
                PESEL = " 11122233344 ",
                InsuranceNumber = " NFZ-UPDATED "
            },
            CancellationToken.None);

        var patient = await dbContext.Patients.FindAsync(1);
        Assert.That(updated, Is.True);
        Assert.That(patient!.PESEL, Is.EqualTo("11122233344"));
        Assert.That(patient.InsuranceNumber, Is.EqualTo("NFZ-UPDATED"));
    }

    private static PatientService CreateService(ClinicDbContext dbContext)
    {
        return new PatientService(
            dbContext,
            new PatientMapper(),
            NullLogger<PatientService>.Instance);
    }

    private static UpsertPatientDto NewPatientDto(string pesel = "12345678901")
    {
        return new UpsertPatientDto
        {
            FirstName = "Piotr",
            LastName = "Testowy",
            PESEL = pesel,
            InsuranceNumber = "NFZ-NEW",
            BirthDate = new DateOnly(1995, 1, 1)
        };
    }
}
