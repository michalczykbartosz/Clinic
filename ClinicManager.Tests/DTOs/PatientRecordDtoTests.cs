using System.ComponentModel.DataAnnotations;
using ClinicManager.Controllers;
using ClinicManager.Controllers.Api;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using ClinicManager.Services;
using ClinicManager.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace ClinicManager.Tests;

public class PatientRecordDtoTests
{
    [Test]
    public void UpdatePatientRecordDto_WhenRequiredFieldsAreInvalid_FailsValidation()
    {
        var dto = new UpdatePatientRecordDto
        {
            PESEL = "abc",
            InsuranceNumber = "   "
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            dto,
            new ValidationContext(dto),
            validationResults,
            validateAllProperties: true);

        var invalidMembers = validationResults.SelectMany(result => result.MemberNames);

        Assert.That(isValid, Is.False);
        Assert.That(invalidMembers, Does.Contain(nameof(UpdatePatientRecordDto.PESEL)));
        Assert.That(invalidMembers, Does.Contain(nameof(UpdatePatientRecordDto.InsuranceNumber)));
    }
}

