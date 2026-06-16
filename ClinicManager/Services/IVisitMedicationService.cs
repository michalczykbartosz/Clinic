using ClinicManager.DTOs;
using ClinicManager.ViewModels;

namespace ClinicManager.Services;

public interface IVisitMedicationService
{
    Task<VisitMedicationsViewModel?> GetForVisitAsync(int visitId, CancellationToken cancellationToken = default);
    Task<CreateVisitMedicationViewModel?> BuildCreateModelAsync(int visitId, CancellationToken cancellationToken = default);
    Task<int?> AddMedicationAsync(CreateVisitMedicationDto dto, CancellationToken cancellationToken = default);
}
