using ClinicManager.Controllers;
using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class ClinicalNoteControllerTests
{
    [Test]
    public async Task GetNote_WhenNoteExists_ReturnsExistingNote()
    {
        var controller = new ClinicalNoteController(
            new StubClinicalNoteService { Note = new ClinicalNoteDto { VisitId = 5, Note = "Kontrola" } },
            NullLogger<ClinicalNoteController>.Instance);

        var result = await controller.GetNote(5) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That((result!.Model as ClinicalNoteDto)!.Note, Is.EqualTo("Kontrola"));
    }

    [Test]
    public async Task GetNote_WhenNoteDoesNotExist_ReturnsEmptyNoteForVisit()
    {
        var controller = new ClinicalNoteController(
            new StubClinicalNoteService(),
            NullLogger<ClinicalNoteController>.Instance);

        var result = await controller.GetNote(7) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That((result!.Model as ClinicalNoteDto)!.VisitId, Is.EqualTo(7));
    }

    [Test]
    public async Task SaveNote_WhenServiceSucceeds_RedirectsToVisitsIndex()
    {
        var service = new StubClinicalNoteService { SaveResult = (true, string.Empty) };
        var controller = new ClinicalNoteController(service, NullLogger<ClinicalNoteController>.Instance);
        var dto = new ClinicalNoteDto { VisitId = 5, Note = "Nowa notatka" };

        var result = await controller.SaveNote(dto);

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        Assert.That(redirect.ControllerName, Is.EqualTo("Visits"));
        Assert.That(service.SavedNote, Is.SameAs(dto));
    }

    private sealed class StubClinicalNoteService : IClinicalNoteService
    {
        public ClinicalNoteDto? Note { get; set; }
        public ClinicalNoteDto? SavedNote { get; private set; }
        public (bool success, string errorMessage) SaveResult { get; set; } = (true, string.Empty);

        public Task<(bool success, ClinicalNoteDto? note, string errorMessage)> GetNoteAsync(int visitId)
        {
            return Task.FromResult((Note is not null, Note, string.Empty));
        }

        public Task<(bool success, string errorMessage)> CreateOrUpdateNoteAsync(ClinicalNoteDto newNote)
        {
            SavedNote = newNote;
            return Task.FromResult(SaveResult);
        }
    }
}
