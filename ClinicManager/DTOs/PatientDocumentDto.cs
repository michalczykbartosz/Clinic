using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ClinicManager.DTOs;

public class PatientDocumentDto
{
    public int PatientDocumentId { get; set; }
    public int PatientId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class PatientDocumentFileDto : PatientDocumentDto
{
    public string PhysicalFilePath { get; set; } = string.Empty;
}

public class UploadPatientDocumentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowy pacjent.")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Wybierz plik dokumentu.")]
    public IFormFile? File { get; set; }
}
