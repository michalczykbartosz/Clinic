using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class DoctorService : IDoctorService
{
    private readonly ClinicDbContext _dbContext;
    private readonly DoctorMapper _doctorMapper;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(ClinicDbContext dbContext, DoctorMapper doctorMapper, ILogger<DoctorService> logger)
    {
        _dbContext = dbContext;
        _doctorMapper = doctorMapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DoctorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var doctors = await _dbContext.Doctors
            .AsNoTracking()
            .OrderBy(doctor => doctor.LastName)
            .ThenBy(doctor => doctor.FirstName)
            .ToListAsync(cancellationToken);

        return doctors.Select(_doctorMapper.ToDto).ToList();
    }

    public async Task<DoctorDto?> GetByIdAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        var doctor = await _dbContext.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(doctor => doctor.DoctorId == doctorId, cancellationToken);

        if (doctor is null)
        {
            _logger.LogWarning("Nie znaleziono lekarza {DoctorId}", doctorId);
            return null;
        }

        return _doctorMapper.ToDto(doctor);
    }
}
