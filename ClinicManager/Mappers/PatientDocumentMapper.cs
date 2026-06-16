using ClinicManager.DTOs;
using ClinicManager.Models;
using Riok.Mapperly.Abstractions;

namespace ClinicManager.Mappers;

[Mapper]
public partial class PatientDocumentMapper
{
    [MapperIgnoreSource(nameof(PatientDocument.Patient))]
    [MapperIgnoreSource(nameof(PatientDocument.StoredFileName))]
    public partial PatientDocumentDto ToDto(PatientDocument document);

    [MapperIgnoreSource(nameof(PatientDocument.Patient))]
    [MapperIgnoreSource(nameof(PatientDocument.StoredFileName))]
    [MapperIgnoreTarget(nameof(PatientDocumentFileDto.PhysicalFilePath))]
    public partial PatientDocumentFileDto ToFileDto(PatientDocument document);
}
