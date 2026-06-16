using System.ComponentModel.DataAnnotations;

namespace ClinicManager.Models;

public class PatientDocument
{
    public int PatientDocumentId { get; set; }

    public int PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string RelativePath { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; }
}
