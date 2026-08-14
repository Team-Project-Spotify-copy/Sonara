using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Sonara.Tests.Infrastructure;

public sealed class SonaraTestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SonaraDbContext> _options;

    public SonaraTestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<SonaraDbContext>()
            .UseSqlite(_connection)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public SonaraDbContext CreateContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
