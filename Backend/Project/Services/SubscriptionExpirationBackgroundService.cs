using Application.Interfaces.Services;

namespace WebApp;

public class SubscriptionExpirationBackgroundService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpirationBackgroundService> _logger;

    public SubscriptionExpirationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionExpirationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

                var expiredCount = await subscriptionService.ExpireOldSubscriptionsAsync(stoppingToken);

                if (expiredCount > 0)
                {
                    _logger.LogInformation("Successfully downgraded {Count} users to Free due to subscription expiration.", expiredCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process expired subscriptions");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}