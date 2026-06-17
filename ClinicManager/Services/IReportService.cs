using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IReportService
{
    Task<(bool success, ReportCostDto?, string error)> GetReportCostAsync(
        int? patientId,
        int? doctorId,
        DateOnly startDate,
        DateOnly endDate,
        ReportCostScope scope,
        CancellationToken cancellationToken = default);
}
