using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

/// <summary>
/// Фабрика контексту для design-time команд (dotnet ef migrations/database).
/// Потрібна, щоб інструменти EF не піднімали весь граф сервісів застосунку.
/// Рядок підключення береться зі змінної середовища SONARA_CONNECTION_STRING,
/// інакше - локальне значення за замовчуванням (для міграцій підключення не використовується).
/// </summary>
public class SonaraDbContextFactory : IDesignTimeDbContextFactory<SonaraDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=sonara;Username=postgres;Password=postgres";

    public SonaraDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("SONARA_CONNECTION_STRING")
            ?? DefaultConnectionString;

        var options = new DbContextOptionsBuilder<SonaraDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new SonaraDbContext(options);
    }
}
