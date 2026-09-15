using Domain.Entities.Users;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
            .ReplaceService<IModelCustomizer, SeedFreeModelCustomizer>()
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();

        // The model's HasData rows are dropped, so the lookup rows the fixtures
        // build their users on are inserted here.
        context.Roles.Add(new Role { Id = TestData.UserRoleId, Name = "User" });
        context.SubscriptionPlans.Add(new SubscriptionPlan
        {
            Id = TestData.FreePlanId,
            Name = "Free",
            Price = 0.00m,
            MaxSlots = 1,
            Features = "Ads included, Audio standard quality, Shuffle only"
        });
        context.SaveChanges();
    }

    public SonaraDbContext CreateContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}

/// <summary>
/// <c>EnsureCreated()</c> replays the model's <c>HasData</c> rows, and those rows
/// contain a cycle — a seeded user points at its active <see cref="UserSubscription"/>,
/// which points back at that user — that the differ cannot order when every row is
/// an insert. Tests build their own fixtures, so the seed rows are dropped here.
/// </summary>
internal sealed class SeedFreeModelCustomizer : RelationalModelCustomizer
{
    public SeedFreeModelCustomizer(ModelCustomizerDependencies dependencies)
        : base(dependencies)
    {
    }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // IMutableEntityType has no API to clear HasData rows; GetRawSeedData()
            // hands back the backing list.
            if (entityType is Microsoft.EntityFrameworkCore.Metadata.Internal.EntityType concrete
                && concrete.GetRawSeedData() is IList<object> rows)
            {
                rows.Clear();
            }
        }
    }
}
