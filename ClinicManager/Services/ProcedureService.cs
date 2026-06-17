using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Models;
using ClinicManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class ProcedureService : IProcedureService
{
    private readonly ClinicDbContext _dbContext;
    private readonly ILogger<ProcedureService> _logger;

    public ProcedureService(ClinicDbContext dbContext, ILogger<ProcedureService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ProcedureListViewModel?> GetForVisitAsync(int visitId, CancellationToken cancellationToken = default)
    {
        var visit = await _dbContext.Visits
            .AsNoTracking()
            .Where(visit => visit.VisitId == visitId)
            .Select(visit => new
            {
                visit.VisitId,
                visit.VisitDateTime,
                visit.PatientId,
                PatientFullName = visit.Patient.FirstName + " " + visit.Patient.LastName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (visit is null)
        {
            return null;
        }

        var procedureRows = await _dbContext.Procedures
            .AsNoTracking()
            .Where(procedure => procedure.MedicalRecord.PatientId == visit.PatientId)
            .OrderByDescending(procedure => procedure.Date)
            .Select(procedure => new
            {
                procedure.ProcedureId,
                procedure.Description,
                procedure.Cost,
                procedure.Date,
                DoctorFullName = procedure.Doctor.FirstName + " " + procedure.Doctor.LastName
            })
            .ToListAsync(cancellationToken);

        var procedures = procedureRows
            .Select(procedure =>
            {
                var (name, description) = SplitStoredDescription(procedure.Description);
                return new ProcedureDto
                {
                    ProcedureId = procedure.ProcedureId,
                    VisitId = visit.VisitId,
                    Name = name,
                    Description = description,
                    Cost = procedure.Cost,
                    Date = procedure.Date,
                    DoctorFullName = procedure.DoctorFullName
                };
            })
            .ToList();

        return new ProcedureListViewModel
        {
            VisitId = visit.VisitId,
            PatientId = visit.PatientId,
            PatientFullName = visit.PatientFullName,
            VisitDateTime = visit.VisitDateTime,
            Procedures = procedures
        };
    }

    public async Task<CreateProcedureDto?> BuildCreateModelAsync(int visitId, CancellationToken cancellationToken = default)
    {
        var visitExists = await _dbContext.Visits.AnyAsync(visit => visit.VisitId == visitId, cancellationToken);
        return visitExists ? new CreateProcedureDto { VisitId = visitId } : null;
    }

    public async Task<int?> CreateAsync(CreateProcedureDto dto, CancellationToken cancellationToken = default)
    {
        var visit = await _dbContext.Visits
            .AsNoTracking()
            .Where(visit => visit.VisitId == dto.VisitId)
            .Select(visit => new { visit.VisitId, visit.PatientId, visit.DoctorId })
            .FirstOrDefaultAsync(cancellationToken);

        if (visit is null)
        {
            return null;
        }

        var medicalRecord = await _dbContext.MedicalRecords
            .FirstOrDefaultAsync(record => record.PatientId == visit.PatientId, cancellationToken);

        if (medicalRecord is null)
        {
            medicalRecord = new MedicalRecord
            {
                PatientId = visit.PatientId,
                Description = string.Empty,
                DescriptionModifyTime = DateTime.Now
            };

            _dbContext.MedicalRecords.Add(medicalRecord);
        }

        var procedure = new Procedure
        {
            DoctorId = visit.DoctorId,
            MedicalRecord = medicalRecord,
            Description = BuildStoredDescription(dto.Name, dto.Description),
            Cost = dto.Cost,
            Date = DateTime.Now
        };

        _dbContext.Procedures.Add(procedure);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Dodano procedure {ProcedureId} do wizyty {VisitId}", procedure.ProcedureId, visit.VisitId);
        return visit.VisitId;
    }

    private static string BuildStoredDescription(string name, string description)
    {
        return $"Nazwa: {name.Trim()}{Environment.NewLine}{Environment.NewLine}{description.Trim()}";
    }

    private static (string Name, string Description) SplitStoredDescription(string storedDescription)
    {
        const string prefix = "Nazwa: ";

        if (!storedDescription.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return ("Procedura", storedDescription);
        }

        var remaining = storedDescription[prefix.Length..];
        var separator = "\r\n\r\n";
        var separatorIndex = remaining.IndexOf(separator, StringComparison.Ordinal);

        if (separatorIndex < 0)
        {
            separator = "\n\n";
            separatorIndex = remaining.IndexOf(separator, StringComparison.Ordinal);
        }

        if (separatorIndex < 0)
        {
            return (remaining, string.Empty);
        }

        return (
            remaining[..separatorIndex],
            remaining[(separatorIndex + separator.Length)..]);
    }
}
