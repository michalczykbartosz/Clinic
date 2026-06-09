using ClinicManager.DTOs;
using ClinicManager.Models;
using Riok.Mapperly.Abstractions;

namespace ClinicManager.Mappers;

[Mapper]
public partial class VisitMapper
{
    [MapperIgnoreSource(nameof(Visit.Patient))]
    [MapperIgnoreSource(nameof(Visit.Doctor))]
    public partial VisitDto ToDto(Visit visit);

    [MapperIgnoreTarget(nameof(Visit.VisitId))]
    [MapperIgnoreTarget(nameof(Visit.VisitStatus))]
    [MapperIgnoreTarget(nameof(Visit.Patient))]
    [MapperIgnoreTarget(nameof(Visit.Doctor))]
    public partial Visit ToEntity(CreateVisitDto dto);
}
