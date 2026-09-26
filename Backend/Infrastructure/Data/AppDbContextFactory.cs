using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;


public class AppDbContextFactory : IDesignTimeDbContextFactory<SonaraDbContext>
{
    private const string UserSecretsId = "d3c16890-da2f-4700-a469-571f3a1b2ea0";

    public SonaraDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(UserSecretsId)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection не знайдено в user-secrets. " +
                "Виконай: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"...\" --project Project");

        var optionsBuilder = new DbContextOptionsBuilder<SonaraDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new SonaraDbContext(optionsBuilder.Options);
    }
}