using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IMedicationService
{
    Task<IReadOnlyList<MedicationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MedicationDto?> GetByIdAsync(int medicationId, CancellationToken cancellationToken = default);
}
