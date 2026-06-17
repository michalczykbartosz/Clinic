using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using ClinicManager.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class ClinicalNoteServiceTests
{
    [Test]
    public async Task GetNoteAsync_WhenNoteDoesNotExist_ReturnsFailure()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        var service = CreateService(dbContext);

        var result = await service.GetNoteAsync(99);

        Assert.That(result.success, Is.False);
        Assert.That(result.note, Is.Null);
    }

    [Test]
    public async Task CreateOrUpdateNoteAsync_WhenNoteDoesNotExist_CreatesNote()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        await SeedVisitAsync(dbContext);
        var service = CreateService(dbContext);

        var result = await service.CreateOrUpdateNoteAsync(new ClinicalNoteDto
        {
            VisitId = 1,
            Note = "Pacjent bez dolegliwosci."
        });

        var note = dbContext.ClinicalNotes.Single();
        Assert.That(result.success, Is.True);
        Assert.That(note.Note, Is.EqualTo("Pacjent bez dolegliwosci."));
    }

    [Test]
    public async Task CreateOrUpdateNoteAsync_WhenNoteExists_UpdatesNote()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        await SeedVisitAsync(dbContext);
        dbContext.ClinicalNotes.Add(new ClinicalNote { VisitId = 1, Note = "Stara notatka" });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        await service.CreateOrUpdateNoteAsync(new ClinicalNoteDto { VisitId = 1, Note = "Nowa notatka" });

        Assert.That(dbContext.ClinicalNotes.Single().Note, Is.EqualTo("Nowa notatka"));
    }

    private static ClinicalNoteService CreateService(ClinicDbContext dbContext)
    {
        return new ClinicalNoteService(
            dbContext,
            new ClinicalNoteMapper(),
            NullLogger<ClinicalNoteService>.Instance);
    }

    private static async Task SeedVisitAsync(ClinicDbContext dbContext)
    {
        await TestData.SeedPeopleAsync(dbContext);
        dbContext.Visits.Add(TestData.Visit());
        await dbContext.SaveChangesAsync();
    }
}
