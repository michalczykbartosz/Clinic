using ClinicManager.Data;
using ClinicManager.Services;
using ClinicManager.Mappers;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class DoctorServiceTests
{
    [Test]
    public async Task GetAllAsync_ReturnsDoctorsOrderedByLastName()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        dbContext.Doctors.AddRange(
            TestData.Doctor(1, "Adam", "Zielinski"),
            TestData.Doctor(2, "Ewa", "Adamska"));
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var doctors = await service.GetAllAsync(CancellationToken.None);

        Assert.That(doctors.Select(doctor => doctor.LastName), Is.EqualTo(new[] { "Adamska", "Zielinski" }));
    }

    [Test]
    public async Task GetByIdAsync_WhenDoctorExists_ReturnsDoctor()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        dbContext.Doctors.Add(TestData.Doctor());
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var doctor = await service.GetByIdAsync(1, CancellationToken.None);

        Assert.That(doctor, Is.Not.Null);
        Assert.That(doctor!.FullName, Is.EqualTo("Adam Wisniewski"));
    }

    [Test]
    public async Task GetByIdAsync_WhenDoctorDoesNotExist_ReturnsNull()
    {
        await using var dbContext = await TestDbContextFactory.CreateAsync();
        var service = CreateService(dbContext);

        var doctor = await service.GetByIdAsync(123, CancellationToken.None);

        Assert.That(doctor, Is.Null);
    }

    private static DoctorService CreateService(ClinicDbContext dbContext)
    {
        return new DoctorService(
            dbContext,
            new DoctorMapper(),
            NullLogger<DoctorService>.Instance);
    }
}
