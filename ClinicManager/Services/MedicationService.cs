using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Models;
using ClinicManager.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class MedicationService : IMedicationService
{
    private readonly ClinicDbContext _dbContext;
    private readonly MedicationMapper _medicationMapper;
    private readonly ILogger<MedicationService> _logger;

    public MedicationService(
        ClinicDbContext dbContext,
        MedicationMapper medicationMapper,
        ILogger<MedicationService> logger)
    {
        _dbContext = dbContext;
        _medicationMapper = medicationMapper;
        _logger = logger;
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

    public async Task<(bool success, string errorMessage)> AddMedicationAsync(MedicationDto newMedicationDto)
    {
        Medication newMedication = _medicationMapper.ToEntity(newMedicationDto);
        await _dbContext.Medications.AddAsync(newMedication);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Dodano lek {MedicationId}: {MedicationName}", newMedication.MedicationId, newMedication.Name);
        return (true, "");

    }

    public async Task<(bool success, string errorMessage)> UpdateMedicationAsync(MedicationDto newMedicationDto)
    {
        Medication newMedication = _medicationMapper.ToEntity(newMedicationDto);
        _dbContext.Medications.Update(newMedication);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Zaktualizowano lek {MedicationId}: {MedicationName}", newMedication.MedicationId, newMedication.Name);
        return (true, "");
    }

    public async Task<(bool success, string errorMessage)> DeleteMedicationAsync(
        int medicationId,
        CancellationToken cancellationToken = default)
    {
        var medication = await _dbContext.Medications
            .FirstOrDefaultAsync(medication => medication.MedicationId == medicationId, cancellationToken);

        if (medication is null)
        {
            return (false, "Nie znaleziono leku do usunięcia.");
        }

        var isUsed = await _dbContext.PrescriptionItems
            .AnyAsync(item => item.MedicationId == medicationId, cancellationToken);

        if (isUsed)
        {
            return (false, "Nie można usunąć leku, który jest już przypisany do wizyty.");
        }

        _dbContext.Medications.Remove(medication);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Usunięto lek {MedicationId}: {MedicationName}", medication.MedicationId, medication.Name);
        return (true, string.Empty);
    }
}
