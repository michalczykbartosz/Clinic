using System.ComponentModel.DataAnnotations;
using ClinicManager.DTOs;

namespace ClinicManager.Tests;

public class VisitMedicationDtoTests
{
    [Test]
    public void CreateVisitMedicationDto_WhenRequiredFieldsAreInvalid_FailsValidation()
    {
        var dto = new CreateVisitMedicationDto
        {
            VisitId = 5,
            MedicationId = 0,
            Dosage = "   ",
            Quantity = 0
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            dto,
            new ValidationContext(dto),
            validationResults,
            validateAllProperties: true);

        var invalidMembers = validationResults.SelectMany(result => result.MemberNames);

        Assert.That(isValid, Is.False);
        Assert.That(invalidMembers, Does.Contain(nameof(CreateVisitMedicationDto.MedicationId)));
        Assert.That(invalidMembers, Does.Contain(nameof(CreateVisitMedicationDto.Dosage)));
        Assert.That(invalidMembers, Does.Contain(nameof(CreateVisitMedicationDto.Quantity)));
    }
}
