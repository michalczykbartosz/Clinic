using ClinicManager.DTOs;
using ClinicManager.ViewModels;

namespace ClinicManager.Services;

public interface IProcedureService
{
    Task<ProcedureListViewModel?> GetForVisitAsync(int visitId, CancellationToken cancellationToken = default);
    Task<CreateProcedureDto?> BuildCreateModelAsync(int visitId, CancellationToken cancellationToken = default);
    Task<int?> CreateAsync(CreateProcedureDto dto, CancellationToken cancellationToken = default);
}
