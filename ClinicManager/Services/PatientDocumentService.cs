using System.Globalization;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class PatientDocumentService : IPatientDocumentService
{
    private const long MaxFileSize = 5 * 1024 * 1024;
    private const int MaxOriginalFileNameLength = 255;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png"
    };

    private readonly ClinicDbContext _dbContext;
    private readonly PatientDocumentMapper _mapper;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PatientDocumentService> _logger;

    public PatientDocumentService(
        ClinicDbContext dbContext,
        PatientDocumentMapper mapper,
        IWebHostEnvironment environment,
        ILogger<PatientDocumentService> logger)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _environment = environment;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PatientDocumentDto>> GetByPatientIdAsync(
        int patientId,
        CancellationToken cancellationToken = default)
    {
        var documents = await _dbContext.PatientDocuments
            .AsNoTracking()
            .Where(document => document.PatientId == patientId)
            .OrderByDescending(document => document.UploadedAt)
            .ToListAsync(cancellationToken);

        return documents.Select(_mapper.ToDto).ToList();
    }

    public async Task<PatientDocumentDto?> UploadAsync(
        UploadPatientDocumentDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.File is null)
        {
            throw new InvalidOperationException("Wybierz plik dokumentu.");
        }

        var patientExists = await _dbContext.Patients
            .AnyAsync(patient => patient.PatientId == dto.PatientId, cancellationToken);

        if (!patientExists)
        {
            return null;
        }

        ValidateFile(dto.File);

        var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
        var originalFileName = NormalizeOriginalFileName(dto.File.FileName, extension);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var patientDirectoryName = dto.PatientId.ToString(CultureInfo.InvariantCulture);
        var relativePath = $"/uploads/patient-documents/{patientDirectoryName}/{storedFileName}";
        var uploadDirectory = Path.Combine(GetWebRootPath(), "uploads", "patient-documents", patientDirectoryName);
        var physicalFilePath = Path.Combine(uploadDirectory, storedFileName);

        try
        {
            Directory.CreateDirectory(uploadDirectory);

            await using var stream = new FileStream(physicalFilePath, FileMode.CreateNew);
            await dto.File.CopyToAsync(stream, cancellationToken);
        }
        catch
        {
            DeletePhysicalFile(physicalFilePath);
            throw;
        }

        var document = new PatientDocument
        {
            PatientId = dto.PatientId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            RelativePath = relativePath,
            ContentType = ResolveContentType(extension),
            FileSize = dto.File.Length,
            UploadedAt = DateTime.UtcNow
        };

        try
        {
            _dbContext.PatientDocuments.Add(document);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            DeletePhysicalFile(physicalFilePath);
            throw;
        }

        _logger.LogInformation(
            "Dodano dokument {PatientDocumentId} do pacjenta {PatientId}",
            document.PatientDocumentId,
            document.PatientId);

        return _mapper.ToDto(document);
    }

    public async Task<PatientDocumentFileDto?> GetFileAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await _dbContext.PatientDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(document => document.PatientDocumentId == documentId, cancellationToken);

        if (document is null)
        {
            return null;
        }

        var fileDto = _mapper.ToFileDto(document);
        fileDto.PhysicalFilePath = GetPhysicalPath(document.RelativePath);

        return File.Exists(fileDto.PhysicalFilePath) ? fileDto : null;
    }

    public async Task<int?> DeleteAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await _dbContext.PatientDocuments
            .FirstOrDefaultAsync(document => document.PatientDocumentId == documentId, cancellationToken);

        if (document is null)
        {
            return null;
        }

        var physicalPath = GetPhysicalPath(document.RelativePath);
        var patientId = document.PatientId;

        _dbContext.PatientDocuments.Remove(document);
        await _dbContext.SaveChangesAsync(cancellationToken);

        DeletePhysicalFile(physicalPath);

        _logger.LogInformation(
            "Usunięto dokument {PatientDocumentId} pacjenta {PatientId}",
            document.PatientDocumentId,
            document.PatientId);

        return patientId;
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("Plik dokumentu jest pusty.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new InvalidOperationException("Plik dokumentu może mieć maksymalnie 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Dozwolone formaty dokumentów to PDF, JPG i PNG.");
        }
    }

    private static string NormalizeOriginalFileName(string fileName, string extension)
    {
        var originalFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            return $"dokument{extension}";
        }

        if (originalFileName.Length <= MaxOriginalFileNameLength)
        {
            return originalFileName;
        }

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var originalExtension = Path.GetExtension(originalFileName);
        var maxNameLength = MaxOriginalFileNameLength - originalExtension.Length;

        if (maxNameLength <= 0)
        {
            return originalFileName[..MaxOriginalFileNameLength];
        }

        var shortenedName = fileNameWithoutExtension[..Math.Min(fileNameWithoutExtension.Length, maxNameLength)];
        return $"{shortenedName}{originalExtension}";
    }

    private static string ResolveContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }

    private string GetPhysicalPath(string relativePath)
    {
        var localPath = relativePath
            .TrimStart('/', '\\')
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        return Path.Combine(GetWebRootPath(), localPath);
    }

    private string GetWebRootPath()
    {
        return _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
    }

    private void DeletePhysicalFile(string physicalPath)
    {
        try
        {
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }
        catch (IOException exception)
        {
            _logger.LogWarning(exception, "Nie udało się usunąć pliku dokumentu {FilePath}", physicalPath);
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogWarning(exception, "Brak uprawnień do usunięcia pliku dokumentu {FilePath}", physicalPath);
        }
    }
}
