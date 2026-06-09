using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class VisitService : IVisitService
{
    private readonly ClinicDbContext _dbContext;
    private readonly VisitMapper _visitMapper;
    private readonly ILogger<VisitService> _logger;

    public VisitService(ClinicDbContext dbContext, VisitMapper visitMapper, ILogger<VisitService> logger)
    {
        _dbContext = dbContext;
        _visitMapper = visitMapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VisitDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var visits = await _dbContext.Visits
            .AsNoTracking()
            .OrderBy(visit => visit.VisitDateTime)
            .ToListAsync(cancellationToken);

        return visits.Select(_visitMapper.ToDto).ToList();
    }

    public async Task<VisitDto?> GetByIdAsync(int visitId, CancellationToken cancellationToken = default)
    {
        var visit = await _dbContext.Visits
            .AsNoTracking()
            .FirstOrDefaultAsync(visit => visit.VisitId == visitId, cancellationToken);

        return visit is null ? null : _visitMapper.ToDto(visit);
    }

    public async Task<IReadOnlyList<VisitDto>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var visits = await _dbContext.Visits
            .AsNoTracking()
            .Where(visit => visit.PatientId == patientId)
            .OrderByDescending(visit => visit.VisitDateTime)
            .ToListAsync(cancellationToken);

        return visits.Select(_visitMapper.ToDto).ToList();
    }

    public async Task<IReadOnlyList<ActiveVisitDto>> GetActiveVisitsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Visits
            .AsNoTracking()
            .Include(visit => visit.Patient)
            .Include(visit => visit.Doctor)
            .Where(visit => visit.VisitStatus == VisitState.Planned || visit.VisitStatus == VisitState.InProgress)
            .OrderBy(visit => visit.VisitDateTime)
            .Select(visit => new ActiveVisitDto
            {
                VisitId = visit.VisitId,
                VisitStatus = visit.VisitStatus,
                VisitDateTime = visit.VisitDateTime,
                PatientId = visit.PatientId,
                PatientFullName = visit.Patient.FirstName + " " + visit.Patient.LastName,
                PatientPESEL = visit.Patient.PESEL,
                DoctorId = visit.DoctorId,
                DoctorFullName = visit.Doctor.FirstName + " " + visit.Doctor.LastName,
                DoctorSpecialization = visit.Doctor.Specialization
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<VisitDto> CreateAsync(CreateVisitDto dto, CancellationToken cancellationToken = default)
    {
        var patientExists = await _dbContext.Patients.AnyAsync(patient => patient.PatientId == dto.PatientId, cancellationToken);
        var doctorExists = await _dbContext.Doctors.AnyAsync(doctor => doctor.DoctorId == dto.DoctorId, cancellationToken);

        if (!patientExists)
        {
            throw new KeyNotFoundException($"Nie znaleziono pacjenta o identyfikatorze {dto.PatientId}.");
        }

        if (!doctorExists)
        {
            throw new KeyNotFoundException($"Nie znaleziono lekarza o identyfikatorze {dto.DoctorId}.");
        }

        var visit = _visitMapper.ToEntity(dto);
        visit.VisitStatus = VisitState.Planned;

        _dbContext.Visits.Add(visit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Utworzono wizytę {VisitId} dla pacjenta {PatientId}", visit.VisitId, visit.PatientId);
        return _visitMapper.ToDto(visit);
    }

    public async Task<bool> UpdateStatusAsync(int visitId, VisitState visitStatus, CancellationToken cancellationToken = default)
    {
        var visit = await _dbContext.Visits
            .FirstOrDefaultAsync(visit => visit.VisitId == visitId, cancellationToken);

        if (visit is null)
        {
            return false;
        }

        visit.VisitStatus = visitStatus;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Zaktualizowano status wizyty {VisitId} na {VisitStatus}", visit.VisitId, visit.VisitStatus);
        return true;
    }
}
