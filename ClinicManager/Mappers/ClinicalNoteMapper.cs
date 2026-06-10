using Riok.Mapperly.Abstractions;
using ClinicManager.Models;
using ClinicManager.DTOs;
namespace ClinicManager.Mappers;

[Mapper]
public partial class ClinicalNoteMapper
{
    public partial ClinicalNoteDto ToDto(ClinicalNote clinicalNote);
    public partial ClinicalNote ToEntity(ClinicalNoteDto dto);
}