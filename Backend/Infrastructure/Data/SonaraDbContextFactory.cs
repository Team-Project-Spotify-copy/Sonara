using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

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
