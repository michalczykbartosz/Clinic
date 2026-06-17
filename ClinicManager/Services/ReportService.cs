using ClinicManager.Data;
using ClinicManager.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class ReportService : IReportService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ClinicDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool success, ReportCostDto?, string error)> GetReportCostAsync(
        int? patientId,
        int? doctorId,
        DateOnly startDate,
        DateOnly endDate,
        ReportCostScope scope,
        CancellationToken cancellationToken = default)
    {
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

        var procedureCost = await _context.Procedures
            .Where(procedure =>
                procedure.Date >= startDateTime &&
                procedure.Date <= endDateTime &&
                (doctorId == null || procedure.DoctorId == doctorId) &&
                (patientId == null || procedure.MedicalRecord.PatientId == patientId))
            .SumAsync(procedure => (decimal?)procedure.Cost, cancellationToken) ?? 0m;

        var visitCost = await _context.Visits
            .Where(visit =>
                visit.VisitDateTime >= startDateTime &&
                visit.VisitDateTime <= endDateTime &&
                (doctorId == null || visit.DoctorId == doctorId) &&
                (patientId == null || visit.PatientId == patientId))
            .SumAsync(visit => (decimal?)visit.Cost, cancellationToken) ?? 0m;

        var result = scope switch
        {
            ReportCostScope.Procedures => procedureCost,
            ReportCostScope.Visits => visitCost,
            ReportCostScope.VisitsAndProcedures => procedureCost + visitCost,
            _ => procedureCost
        };

        var raportDto = new ReportCostDto
        {
            OverallCost = result,
            ProcedureCost = procedureCost,
            VisitCost = visitCost,
            Scope = scope,
            StartDate = startDate,
            EndDate = endDate,
            DoctorId = doctorId ?? 0,
            PatientId = patientId ?? 0
        };

        _logger.LogInformation(
            "Obliczono raport kosztów: PatientId={PatientId}, DoctorId={DoctorId}, StartDate={StartDate}, EndDate={EndDate}, Scope={Scope}, Cost={Cost}",
            patientId,
            doctorId,
            startDate,
            endDate,
            scope,
            result);

        return (true, raportDto, string.Empty);
    }
}
