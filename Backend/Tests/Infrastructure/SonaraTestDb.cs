using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Sonara.Tests.Infrastructure;

/// <summary>
/// Одноразова база на SQLite in-memory. Використовується реляційний провайдер (а не InMemory),
/// щоб перевірялися справжні складені ключі, зовнішні ключі та трансляція LINQ у SQL.
/// EnsureCreated застосовує ту саму модель і ті самі seed-дані, що й продакшн-контекст.
/// </summary>
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

    /// <summary>Новий контекст на ту саму базу - імітує окремий запит без спільного кешу відстеження.</summary>
    public SonaraDbContext CreateContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
