using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class PatientService : IPatientService
{
    private readonly ClinicDbContext _dbContext;
    private readonly PatientMapper _patientMapper;
    private readonly ILogger<PatientService> _logger;

    public PatientService(ClinicDbContext dbContext, PatientMapper patientMapper, ILogger<PatientService> logger)
    {
        _dbContext = dbContext;
        _patientMapper = patientMapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PatientDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var patients = await _dbContext.Patients
            .AsNoTracking()
            .OrderBy(patient => patient.LastName)
            .ThenBy(patient => patient.FirstName)
            .ToListAsync(cancellationToken);

        return patients.Select(_patientMapper.ToDto).ToList();
    }

    public async Task<IReadOnlyList<PatientDto>> SearchAsync(string? query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync(cancellationToken);
        }

        var trimmedQuery = query.Trim();

        var patients = await _dbContext.Patients
            .AsNoTracking()
            .Where(patient => patient.LastName.Contains(trimmedQuery) || patient.PESEL.Contains(trimmedQuery))
            .OrderBy(patient => patient.LastName)
            .ThenBy(patient => patient.FirstName)
            .ToListAsync(cancellationToken);

        return patients.Select(_patientMapper.ToDto).ToList();
    }

    public async Task<PatientDto?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(patient => patient.PatientId == patientId, cancellationToken);

        return patient is null ? null : _patientMapper.ToDto(patient);
    }

    public async Task<PatientDto> CreateAsync(UpsertPatientDto dto, CancellationToken cancellationToken = default)
    {
        var patient = _patientMapper.ToEntity(dto);

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Utworzono pacjenta {PatientId}", patient.PatientId);
        return _patientMapper.ToDto(patient);
    }

    public async Task<bool> UpdateAsync(int patientId, UpsertPatientDto dto, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.Patients
            .FirstOrDefaultAsync(patient => patient.PatientId == patientId, cancellationToken);

        if (patient is null)
        {
            return false;
        }

        _patientMapper.UpdateEntity(dto, patient);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Zaktualizowano pacjenta {PatientId}", patient.PatientId);
        return true;
    }

    public async Task<bool> DeleteAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.Patients
            .FirstOrDefaultAsync(patient => patient.PatientId == patientId, cancellationToken);

        if (patient is null)
        {
            return false;
        }

        _dbContext.Patients.Remove(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Usunięto pacjenta {PatientId}", patient.PatientId);
        return true;
    }
}
