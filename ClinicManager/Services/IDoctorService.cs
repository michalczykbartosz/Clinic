using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IDoctorService
{
    Task<IReadOnlyList<DoctorDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DoctorDto?> GetByIdAsync(int doctorId, CancellationToken cancellationToken = default);
}
