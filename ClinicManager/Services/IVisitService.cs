using ClinicManager.DTOs;
using ClinicManager.Models;

namespace ClinicManager.Services;

public interface IVisitService
{
    Task<IReadOnlyList<VisitDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VisitListItemDto>> GetListAsync(string? query = null, VisitState? status = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VisitListItemDto>> GetListForPatientPeselAsync(string pesel, VisitState? status = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VisitListItemDto>> GetDoctorScheduleAsync(int doctorId, DateOnly date, CancellationToken cancellationToken = default);
    Task<VisitDto?> GetByIdAsync(int visitId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientVisitDto>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActiveVisitDto>> GetActiveVisitsAsync(CancellationToken cancellationToken = default);
    Task<VisitDto> CreateAsync(CreateVisitDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(int visitId, VisitState visitStatus, CancellationToken cancellationToken = default);
    Task<bool> UpdatePaymentAsync(int visitId, bool isPaid, CancellationToken cancellationToken = default);
}
