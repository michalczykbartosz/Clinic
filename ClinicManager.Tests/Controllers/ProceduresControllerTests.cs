using ClinicManager.Controllers;
using ClinicManager.DTOs;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class ProceduresControllerTests
{
    [Test]
    public async Task Index_WhenVisitExists_ReturnsProcedureList()
    {
        var controller = new ProceduresController(
            new StubProcedureService { ListModel = new ProcedureListViewModel { VisitId = 5 } },
            NullLogger<ProceduresController>.Instance);

        var result = await controller.Index(5, CancellationToken.None) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That((result!.Model as ProcedureListViewModel)!.VisitId, Is.EqualTo(5));
    }

    [Test]
    public async Task Create_Get_WhenVisitDoesNotExist_ReturnsNotFound()
    {
        var controller = new ProceduresController(
            new StubProcedureService(),
            NullLogger<ProceduresController>.Instance);

        var result = await controller.Create(99, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_Post_WhenServiceSucceeds_RedirectsToIndex()
    {
        var service = new StubProcedureService { CreatedVisitId = 5 };
        var controller = new ProceduresController(service, NullLogger<ProceduresController>.Instance)
        {
            TempData = ControllerTestHelpers.TempData()
        };
        var dto = new CreateProcedureDto { VisitId = 5, Name = "Skaling", Description = "Opis", Cost = 120m };

        var result = await controller.Create(dto, CancellationToken.None);

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        Assert.That(redirect.RouteValues!["visitId"], Is.EqualTo(5));
        Assert.That(service.CreatedProcedure, Is.SameAs(dto));
    }

    private sealed class StubProcedureService : IProcedureService
    {
        public ProcedureListViewModel? ListModel { get; set; }
        public CreateProcedureDto? CreateModel { get; set; }
        public int? CreatedVisitId { get; set; }
        public CreateProcedureDto? CreatedProcedure { get; private set; }

        public Task<ProcedureListViewModel?> GetForVisitAsync(int visitId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ListModel);
        }

        public Task<CreateProcedureDto?> BuildCreateModelAsync(int visitId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateModel);
        }

        public Task<int?> CreateAsync(CreateProcedureDto dto, CancellationToken cancellationToken = default)
        {
            CreatedProcedure = dto;
            return Task.FromResult(CreatedVisitId);
        }
    }
}
