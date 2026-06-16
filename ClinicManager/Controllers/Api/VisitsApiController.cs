using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.Controllers.Api;

[ApiController]
[Route("api/visits")]
[Produces("application/json")]
public class VisitsApiController : ControllerBase
{
    private readonly IVisitService _visitService;
    private readonly ILogger<VisitsApiController> _logger;

    public VisitsApiController(IVisitService visitService, ILogger<VisitsApiController> logger)
    {
        _visitService = visitService;
        _logger = logger;
    }

    /// <summary>
    /// Zwraca aktywne wizyty z danymi pacjenta i lekarza.
    /// </summary>
    /// <remarks>
    /// Endpoint korzysta z danych z tabel Visits, Patients i Doctors.
    /// Aktywne wizyty to wizyty zaplanowane albo będące w trakcie.
    /// </remarks>
    [HttpGet("active")]
    [EndpointSummary("Lista aktywnych wizyt")]
    [EndpointDescription("Zwraca aktywne wizyty z danymi pacjenta i lekarza pobranymi z powiązanych tabel bazy danych.")]
    [ProducesResponseType(typeof(IReadOnlyList<ActiveVisitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ActiveVisitDto>>> GetActiveVisits(
        CancellationToken cancellationToken)
    {
        var visits = await _visitService.GetActiveVisitsAsync(cancellationToken);

        _logger.LogInformation("Pobrano aktywne wizyty przez API. Liczba wyników: {VisitCount}", visits.Count);
        return Ok(visits);
    }
}
