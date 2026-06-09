using ClinicManager.DTOs;
using ClinicManager.Models;
using Riok.Mapperly.Abstractions;

namespace ClinicManager.Mappers;

[Mapper]
public partial class PatientMapper
{
    [MapperIgnoreSource(nameof(Patient.VisitList))]
    public partial PatientDto ToDto(Patient patient);

    [MapperIgnoreTarget(nameof(Patient.PatientId))]
    [MapperIgnoreTarget(nameof(Patient.VisitList))]
    public partial Patient ToEntity(UpsertPatientDto dto);

    [MapperIgnoreTarget(nameof(Patient.PatientId))]
    [MapperIgnoreTarget(nameof(Patient.VisitList))]
    public partial void UpdateEntity(UpsertPatientDto dto, Patient patient);
}
