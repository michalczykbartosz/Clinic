using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IPatientDocumentService
{
    Task<IReadOnlyList<PatientDocumentDto>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<PatientDocumentDto?> UploadAsync(UploadPatientDocumentDto dto, CancellationToken cancellationToken = default);
    Task<PatientDocumentFileDto?> GetFileAsync(int documentId, CancellationToken cancellationToken = default);
    Task<int?> DeleteAsync(int documentId, CancellationToken cancellationToken = default);
}
