using ClinicManager.Controllers;
using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class MedicationControllerTests
{
    [Test]
    public async Task Index_ReturnsMedicationListFromService()
    {
        var controller = new MedicationController(
            new StubMedicationService
            {
                Medications = [Medication(1, "Apap")]
            },
            NullLogger<MedicationController>.Instance);

        var result = await controller.Index() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Model, Is.InstanceOf<IReadOnlyList<MedicationDto>>());
        Assert.That((result.Model as IReadOnlyList<MedicationDto>)![0].Name, Is.EqualTo("Apap"));
    }

    [Test]
    public async Task Edit_Get_WhenMedicationDoesNotExist_RedirectsToIndex()
    {
        var controller = new MedicationController(
            new StubMedicationService(),
            NullLogger<MedicationController>.Instance);

        var result = await controller.Edit(99);

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
    }

    [Test]
    public async Task Save_Post_WhenServiceSucceeds_RedirectsToIndex()
    {
        var service = new StubMedicationService { AddResult = (true, string.Empty) };
        var controller = new MedicationController(service, NullLogger<MedicationController>.Instance);
        var dto = Medication(0, "Ibuprom");

        var result = await controller.Save(dto);

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        Assert.That(service.AddedMedication, Is.SameAs(dto));
    }

    private static MedicationDto Medication(int id, string name)
    {
        return new MedicationDto
        {
            MedicationId = id,
            Name = name,
            Manufacturer = "USP",
            Dose = "500mg"
        };
    }

    private sealed class StubMedicationService : IMedicationService
    {
        public IReadOnlyList<MedicationDto> Medications { get; set; } = [];
        public MedicationDto? Medication { get; set; }
        public MedicationDto? AddedMedication { get; private set; }
        public (bool success, string errorMessage) AddResult { get; set; } = (true, string.Empty);

        public Task<IReadOnlyList<MedicationDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Medications);
        }

        public Task<MedicationDto?> GetByIdAsync(int medicationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Medication);
        }

        public Task<(bool success, string errorMessage)> AddMedicationAsync(MedicationDto newMedicationDto)
        {
            AddedMedication = newMedicationDto;
            return Task.FromResult(AddResult);
        }

        public Task<(bool success, string errorMessage)> UpdateMedicationAsync(MedicationDto newMedicationDto)
        {
            return Task.FromResult((true, string.Empty));
        }
    }
}
