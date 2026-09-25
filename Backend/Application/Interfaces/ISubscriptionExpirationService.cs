namespace Application.Interfaces;

public interface ISubscriptionExpirationService
{
    Task<int> ExpireOldSubscriptionsAsync(CancellationToken ct = default);
}