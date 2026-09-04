using Application.Interfaces;

namespace WebApp.Services;

public class SubscriptionReminderBackgroundService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionReminderBackgroundService> _logger;

    public SubscriptionReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var reminderService = scope.ServiceProvider.GetRequiredService<ISubscriptionReminderService>();
                    await reminderService.SendWeeklyRemindersAsync(force: false, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check/send weekly reminders");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}