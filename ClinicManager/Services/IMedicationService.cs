using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IMedicationService
{
    Task<IReadOnlyList<MedicationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MedicationDto?> GetByIdAsync(int medicationId, CancellationToken cancellationToken = default);

    Task<(bool success,string errorMessage)> AddMedicationAsync(MedicationDto newMedicationDto);

    Task<(bool success, string errorMessage)> UpdateMedicationAsync(MedicationDto newMedicationDto);
}
