using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
public class PatientDocumentsController : Controller
{
    private readonly IPatientDocumentService _documentService;
    private readonly ILogger<PatientDocumentsController> _logger;

    public PatientDocumentsController(
        IPatientDocumentService documentService,
        ILogger<PatientDocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadPatientDocumentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Nie udało się dodać dokumentu. Wybierz poprawny plik.";
            return RedirectToPatient(dto.PatientId);
        }

        try
        {
            var document = await _documentService.UploadAsync(dto, cancellationToken);
            if (document is null)
            {
                _logger.LogWarning("Nie znaleziono pacjenta {PatientId} podczas dodawania dokumentu.", dto.PatientId);
                return NotFound();
            }

            TempData["SuccessMessage"] = "Dokument został dodany do kartoteki pacjenta.";
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "Nie udało się dodać dokumentu pacjenta {PatientId}.", dto.PatientId);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToPatient(dto.PatientId);
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var document = await _documentService.GetFileAsync(id, cancellationToken);
        if (document is null)
        {
            _logger.LogWarning("Nie znaleziono pliku dokumentu {PatientDocumentId}.", id);
            return NotFound();
        }

        return PhysicalFile(document.PhysicalFilePath, document.ContentType, document.OriginalFileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var patientId = await _documentService.DeleteAsync(id, cancellationToken);
        if (patientId is null)
        {
            _logger.LogWarning("Nie znaleziono dokumentu {PatientDocumentId} do usunięcia.", id);
            return NotFound();
        }

        TempData["SuccessMessage"] = "Dokument został usunięty z kartoteki pacjenta.";
        return RedirectToPatient(patientId.Value);
    }

    private RedirectToActionResult RedirectToPatient(int patientId)
    {
        return RedirectToAction("Details", "Patients", new { id = patientId });
    }
}
