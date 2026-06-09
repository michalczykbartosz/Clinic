using ClinicManager.DTOs;
using ClinicManager.Models;
using Riok.Mapperly.Abstractions;

namespace ClinicManager.Mappers;

[Mapper]
public partial class DoctorMapper
{
    [MapperIgnoreSource(nameof(Doctor.Procedures))]
    [MapperIgnoreSource(nameof(Doctor.Visits))]
    public partial DoctorDto ToDto(Doctor doctor);
}
