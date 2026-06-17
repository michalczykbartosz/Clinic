using ClinicManager.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Tests;

internal sealed class TestClinicDbContext : ClinicDbContext
{
    private readonly SqliteConnection _connection;

    public TestClinicDbContext(DbContextOptions<ClinicDbContext> options, SqliteConnection connection)
        : base(options)
    {
        _connection = connection;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
