using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class MedicationService : IMedicationService
{
    private readonly ClinicDbContext _dbContext;
    private readonly MedicationMapper _medicationMapper;

    public MedicationService(ClinicDbContext dbContext, MedicationMapper medicationMapper)
    {
        _dbContext = dbContext;
        _medicationMapper = medicationMapper;
    }

    public async Task<IReadOnlyList<MedicationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var medications = await _dbContext.Medications
            .AsNoTracking()
            .OrderBy(medication => medication.Name)
            .ToListAsync(cancellationToken);

        return medications.Select(_medicationMapper.ToDto).ToList();
    }

    public async Task<MedicationDto?> GetByIdAsync(int medicationId, CancellationToken cancellationToken = default)
    {
        var medication = await _dbContext.Medications
            .AsNoTracking()
            .FirstOrDefaultAsync(medication => medication.MedicationId == medicationId, cancellationToken);

        return medication is null ? null : _medicationMapper.ToDto(medication);
    }
}
