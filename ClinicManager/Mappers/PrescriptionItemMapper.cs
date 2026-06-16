using ClinicManager.DTOs;
using ClinicManager.Models;
using Riok.Mapperly.Abstractions;

namespace ClinicManager.Mappers;

[Mapper]
public partial class PrescriptionItemMapper
{
    [MapProperty(nameof(CreateVisitMedicationDto.Dosage), nameof(PrescriptionItem.Description))]
    [MapperIgnoreSource(nameof(CreateVisitMedicationDto.VisitId))]
    [MapperIgnoreTarget(nameof(PrescriptionItem.PrescriptionItemId))]
    [MapperIgnoreTarget(nameof(PrescriptionItem.PrescriptionId))]
    [MapperIgnoreTarget(nameof(PrescriptionItem.Prescription))]
    [MapperIgnoreTarget(nameof(PrescriptionItem.Medication))]
    public partial PrescriptionItem ToEntity(CreateVisitMedicationDto dto);
}
