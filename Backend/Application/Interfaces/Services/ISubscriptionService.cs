using Application.DTOs.Subscription;

namespace Application.Interfaces.Services;

public interface ISubscriptionService
{
    Task<IReadOnlyList<SubscriptionDto>> GetAllSubscriptionsAsync(CancellationToken ct = default); 
    Task<SubscriptionDto?> GetSubscriptionsByIdAsync(Guid subscriptionId, CancellationToken ct = default);
    Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionDto dto, CancellationToken ct = default);
    Task<SubscriptionDto?> UpdateSubscriptionAsync(Guid subscriptionId, UpdateSubscriptionDto dto, CancellationToken ct = default);
    Task<bool> RemoveSubscriptionAsync(Guid subscriptionId, CancellationToken ct = default);
    Task<SubscriptionDto?> GetUserSubscriptionAsync(Guid userId, CancellationToken ct = default);
}
