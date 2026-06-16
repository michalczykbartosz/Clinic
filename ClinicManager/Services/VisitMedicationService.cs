using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class VisitMedicationService : IVisitMedicationService
{
    private readonly ClinicDbContext _dbContext;
    private readonly PrescriptionItemMapper _prescriptionItemMapper;
    private readonly ILogger<VisitMedicationService> _logger;

    public VisitMedicationService(
        ClinicDbContext dbContext,
        PrescriptionItemMapper prescriptionItemMapper,
        ILogger<VisitMedicationService> logger)
    {
        _dbContext = dbContext;
        _prescriptionItemMapper = prescriptionItemMapper;
        _logger = logger;
    }

    public async Task<VisitMedicationsViewModel?> GetForVisitAsync(
        int visitId,
        CancellationToken cancellationToken = default)
    {
        var visit = await _dbContext.Visits
            .AsNoTracking()
            .Where(visit => visit.VisitId == visitId)
            .Select(visit => new
            {
                visit.VisitId,
                visit.VisitDateTime,
                visit.PatientId,
                PatientFullName = visit.Patient.FirstName + " " + visit.Patient.LastName,
                DoctorFullName = visit.Doctor.FirstName + " " + visit.Doctor.LastName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (visit is null)
        {
            return null;
        }

        var medications = await _dbContext.PrescriptionItems
            .AsNoTracking()
            .Where(item => item.Prescription.VisitId == visitId)
            .OrderBy(item => item.Medication.Name)
            .ThenBy(item => item.Medication.Dose)
            .Select(item => new VisitMedicationDto
            {
                PrescriptionItemId = item.PrescriptionItemId,
                MedicationId = item.MedicationId,
                MedicationName = item.Medication.Name,
                Manufacturer = item.Medication.Manufacturer,
                Dose = item.Medication.Dose,
                Dosage = item.Description,
                Quantity = item.Quantity
            })
            .ToListAsync(cancellationToken);

        return new VisitMedicationsViewModel
        {
            VisitId = visit.VisitId,
            PatientId = visit.PatientId,
            PatientFullName = visit.PatientFullName,
            DoctorFullName = visit.DoctorFullName,
            VisitDateTime = visit.VisitDateTime,
            Medications = medications
        };
    }

    public async Task<CreateVisitMedicationViewModel?> BuildCreateModelAsync(
        int visitId,
        CancellationToken cancellationToken = default)
    {
        var visitExists = await _dbContext.Visits
            .AnyAsync(visit => visit.VisitId == visitId, cancellationToken);

        if (!visitExists)
        {
            return null;
        }

        var medications = await _dbContext.Medications
            .AsNoTracking()
            .OrderBy(medication => medication.Name)
            .ThenBy(medication => medication.Dose)
            .Select(medication => new SelectListItem
            {
                Value = medication.MedicationId.ToString(),
                Text = medication.Name + " - " + medication.Dose + " (" + medication.Manufacturer + ")"
            })
            .ToListAsync(cancellationToken);

        return new CreateVisitMedicationViewModel
        {
            Medication = new CreateVisitMedicationDto { VisitId = visitId },
            AvailableMedications = medications
        };
    }

    public async Task<int?> AddMedicationAsync(
        CreateVisitMedicationDto dto,
        CancellationToken cancellationToken = default)
    {
        var visitExists = await _dbContext.Visits
            .AnyAsync(visit => visit.VisitId == dto.VisitId, cancellationToken);

        if (!visitExists)
        {
            return null;
        }

        var medicationExists = await _dbContext.Medications
            .AnyAsync(medication => medication.MedicationId == dto.MedicationId, cancellationToken);

        if (!medicationExists)
        {
            return null;
        }

        var prescription = await _dbContext.Prescriptions
            .FirstOrDefaultAsync(prescription => prescription.VisitId == dto.VisitId, cancellationToken);

        if (prescription is null)
        {
            prescription = new Prescription
            {
                VisitId = dto.VisitId,
                PrescriptionItems = []
            };

            _dbContext.Prescriptions.Add(prescription);
        }

        var prescriptionItem = _prescriptionItemMapper.ToEntity(dto);
        prescriptionItem.Description = dto.Dosage.Trim();

        if (prescription.PrescriptionId > 0)
        {
            prescriptionItem.PrescriptionId = prescription.PrescriptionId;
        }
        else
        {
            prescriptionItem.Prescription = prescription;
        }

        _dbContext.PrescriptionItems.Add(prescriptionItem);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Przypisano lek {MedicationId} do wizyty {VisitId} w ilości {Quantity}",
            dto.MedicationId,
            dto.VisitId,
            dto.Quantity);

        return dto.VisitId;
    }
}
