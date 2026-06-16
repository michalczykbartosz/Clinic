using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IPatientService
{
    Task<IReadOnlyList<PatientDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientDto>> SearchAsync(string? query, CancellationToken cancellationToken = default);
    Task<PatientDto?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<PatientDto> CreateAsync(UpsertPatientDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int patientId, UpsertPatientDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateRecordAsync(int patientId, UpdatePatientRecordDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int patientId, CancellationToken cancellationToken = default);
}
