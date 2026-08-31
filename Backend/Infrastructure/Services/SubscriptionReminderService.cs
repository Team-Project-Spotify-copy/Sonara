using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class SubscriptionReminderService : ISubscriptionReminderService
{
    private const string SentFlagKey = "jobs:weekly-subscription-reminder:sent";
    private static readonly TimeSpan SentFlagTtl = TimeSpan.FromDays(8);

    private readonly SonaraDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cache;
    private readonly ILogger<SubscriptionReminderService> _logger;

    public SubscriptionReminderService(
        SonaraDbContext db,
        IEmailService emailService,
        ICacheService cache,
        ILogger<SubscriptionReminderService> logger)
    {
        _db = db;
        _emailService = emailService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<int> SendWeeklyRemindersAsync(bool force = false, CancellationToken ct = default)
    {
        if (!force)
        {
            var alreadySent = await _cache.GetAsync<bool?>(SentFlagKey, ct);
            if (alreadySent == true)
            {
                _logger.LogInformation("The weekly newsletter has already been sent out this week, so I’m skipping it.");
                return 0;
            }
        }

        var now = DateTime.UtcNow;

        var recipients = await _db.Users
            .Include(u => u.ActiveSubscription)
                .ThenInclude(s => s!.Plan)
            .Where(u => u.ActiveSubscription == null
                     || u.ActiveSubscription.Plan.Price == 0
                     || u.ActiveSubscription.ExpiresAt < now)
            .Select(u => new { u.Email, u.Username })
            .ToListAsync(ct);

        _logger.LogInformation("Weekly subscription reminder newsletter: {Count} recipients", recipients.Count);

        var sentCount = 0;
        foreach (var user in recipients)
        {
            try
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "You're missing Sonara Premium",
                    BuildReminderHtml(user.Username),
                    ct);
                sentCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send reminder to {Email}", user.Email);
            }
        }

        await _cache.SetAsync(SentFlagKey, true, SentFlagTtl, ct);
        return sentCount;
    }

    private static string BuildReminderHtml(string username)
    {
        return $"""
            <p>Hello, {username}!</p>
            <p>You're still on the free plan of Sonara. Upgrade to Premium to listen
            without ads, in high quality, and offline.</p>
            <p>— The Sonara Team</p>
            """;
    }
}