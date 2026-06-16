using ClinicManager.DTOs;
using ClinicManager.Models;
using Riok.Mapperly.Abstractions;

namespace ClinicManager.Mappers;

[Mapper]
public partial class PatientMapper
{
    [MapperIgnoreSource(nameof(Patient.VisitList))]
    [MapperIgnoreSource(nameof(Patient.Documents))]
    public partial PatientDto ToDto(Patient patient);

    [MapperIgnoreTarget(nameof(Patient.PatientId))]
    [MapperIgnoreTarget(nameof(Patient.VisitList))]
    [MapperIgnoreTarget(nameof(Patient.Documents))]
    public partial Patient ToEntity(UpsertPatientDto dto);

    [MapperIgnoreTarget(nameof(Patient.PatientId))]
    [MapperIgnoreTarget(nameof(Patient.VisitList))]
    [MapperIgnoreTarget(nameof(Patient.Documents))]
    public partial void UpdateEntity(UpsertPatientDto dto, Patient patient);
}
