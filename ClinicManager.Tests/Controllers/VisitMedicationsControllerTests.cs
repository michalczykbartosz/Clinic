using System.ComponentModel.DataAnnotations;
using ClinicManager.Controllers;
using ClinicManager.Controllers.Api;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace ClinicManager.Tests;

public class VisitMedicationsControllerTests
{
    [Test]
    public async Task Index_WhenVisitExists_ReturnsMedicationList()
    {
        var service = new StubVisitMedicationService
        {
            Model = new VisitMedicationsViewModel
            {
                VisitId = 5,
                PatientId = 1,
                PatientFullName = "Jan Nowak",
                DoctorFullName = "Adam WiÅ›niewski",
                VisitDateTime = new DateTime(2026, 6, 16, 10, 0, 0),
                Medications =
                [
                    new VisitMedicationDto
                    {
                        PrescriptionItemId = 3,
                        MedicationId = 2,
                        MedicationName = "Ibuprom Max",
                        Manufacturer = "US Pharmacia",
                        Dose = "400mg",
                        Dosage = "1 tabletka rano",
                        Quantity = 1
                    }
                ]
            }
        };

        var controller = new VisitMedicationsController(
            service,
            NullLogger<VisitMedicationsController>.Instance);

        var result = await controller.Index(5, CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);

        var model = viewResult!.Model as VisitMedicationsViewModel;
        Assert.That(model, Is.Not.Null);
        Assert.That(model!.Medications, Has.Count.EqualTo(1));
        Assert.That(model.Medications[0].Dosage, Is.EqualTo("1 tabletka rano"));
    }

    [Test]
    public async Task Index_WhenVisitDoesNotExist_ReturnsNotFound()
    {
        var controller = new VisitMedicationsController(
            new StubVisitMedicationService(),
            NullLogger<VisitMedicationsController>.Instance);

        var result = await controller.Index(999, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_Get_WhenVisitExists_ReturnsCreateForm()
    {
        var controller = new VisitMedicationsController(
            new StubVisitMedicationService(),
            NullLogger<VisitMedicationsController>.Instance);

        var result = await controller.Create(5, CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);

        var model = viewResult!.Model as CreateVisitMedicationViewModel;
        Assert.That(model, Is.Not.Null);
        Assert.That(model!.Medication.VisitId, Is.EqualTo(5));
    }

    [Test]
    public async Task Create_Post_WhenMedicationIsValid_AddsMedicationAndRedirectsToIndex()
    {
        var service = new StubVisitMedicationService { AddedVisitId = 5 };
        var controller = new VisitMedicationsController(
            service,
            NullLogger<VisitMedicationsController>.Instance)
        {
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                new TestTempDataProvider())
        };

        var model = new CreateVisitMedicationViewModel
        {
            Medication = new CreateVisitMedicationDto
            {
                VisitId = 5,
                MedicationId = 2,
                Dosage = "1 tabletka rano",
                Quantity = 2
            }
        };

        var result = await controller.Create(model, CancellationToken.None);

        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Index"));
        Assert.That(redirectResult.RouteValues!["visitId"], Is.EqualTo(5));
        Assert.That(service.AddedMedication, Is.Not.Null);
        Assert.That(service.AddedMedication!.MedicationId, Is.EqualTo(2));
        Assert.That(service.AddedMedication.Quantity, Is.EqualTo(2));
    }

    private sealed class StubVisitMedicationService : IVisitMedicationService
    {
        public VisitMedicationsViewModel? Model { get; set; }
        public int? AddedVisitId { get; set; }
        public CreateVisitMedicationDto? AddedMedication { get; private set; }

        public Task<VisitMedicationsViewModel?> GetForVisitAsync(int visitId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Model);
        }

        public Task<CreateVisitMedicationViewModel?> BuildCreateModelAsync(int visitId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<CreateVisitMedicationViewModel?>(new CreateVisitMedicationViewModel
            {
                Medication = new CreateVisitMedicationDto { VisitId = visitId }
            });
        }

        public Task<int?> AddMedicationAsync(CreateVisitMedicationDto dto, CancellationToken cancellationToken = default)
        {
            AddedMedication = dto;
            return Task.FromResult(AddedVisitId);
        }
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            return new Dictionary<string, object>();
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
