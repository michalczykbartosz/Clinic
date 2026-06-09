using ClinicManager.DTOs;
using ClinicManager.Models;
using Riok.Mapperly.Abstractions;

namespace ClinicManager.Mappers;

[Mapper]
public partial class MedicationMapper
{
    public partial MedicationDto ToDto(Medication medication);
}
