namespace Application.Interfaces;

public interface ISubscriptionReminderService
{
    Task<int> SendWeeklyRemindersAsync(bool force = false, CancellationToken ct = default);
}