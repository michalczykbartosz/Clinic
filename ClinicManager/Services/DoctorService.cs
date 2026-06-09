using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class DoctorService : IDoctorService
{
    private readonly ClinicDbContext _dbContext;
    private readonly DoctorMapper _doctorMapper;

    public DoctorService(ClinicDbContext dbContext, DoctorMapper doctorMapper)
    {
        _dbContext = dbContext;
        _doctorMapper = doctorMapper;
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

        return doctor is null ? null : _doctorMapper.ToDto(doctor);
    }
}
