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
    
    public async Task<(bool success, ReportCostDto?, string error)> GetReportCostAsync(int? patientId, int? doctorId,
        DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        DateTime startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        DateTime endDateTime = endDate.ToDateTime(TimeOnly.MinValue);
        var result = await _context.Procedures.Where(x => x.Date.Date >= startDateTime && x.Date.Date <= endDateTime &&
                                                          (doctorId == null || x.DoctorId == doctorId) &&
                                                          (patientId == null || x.MedicalRecord.PatientId == patientId))
            .SumAsync(p => (decimal?)p.Cost, cancellationToken) ?? 0;

        var raportDto = new ReportCostDto
        {
            OverallCost = result,
            StartDate = startDate,
            EndDate = endDate,
            DoctorId = doctorId ?? 0,
            PatientId = patientId ?? 0
        };

        _logger.LogInformation(
            "Obliczono raport kosztów: PatientId={PatientId}, DoctorId={DoctorId}, StartDate={StartDate}, EndDate={EndDate}, Cost={Cost}",
            patientId,
            doctorId,
            startDate,
            endDate,
            result);

        return (true, raportDto, string.Empty);
    }
    
}
