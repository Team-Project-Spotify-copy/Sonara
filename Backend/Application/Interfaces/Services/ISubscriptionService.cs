using Application.DTOs.Subscription;
namespace Application.Interfaces.Services;
public interface ISubscriptionService
{
    Task<IReadOnlyList<SubscriptionPlanDto>> GetAllPlansAsync(CancellationToken ct = default);
    Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid Id, CancellationToken ct = default);
    Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto, CancellationToken ct = default);
    Task<SubscriptionPlanDto?> UpdatePlanAsync(Guid Id, UpdateSubscriptionPlanDto dto, CancellationToken ct = default);
    Task<bool> RemovePlanAsync(Guid Id, CancellationToken ct = default);

    Task<UserSubscriptionDto?> GetUserSubscriptionAsync(Guid Id, CancellationToken ct = default);
    Task<UserSubscriptionDto> ProcessBlockchainPurchaseAsync(Guid Id, byte planTypeByte, CancellationToken ct = default);
    Task<bool> InviteToSubscriptionAsync(Guid ownerId, string targetUsername, CancellationToken ct = default);
    Task<bool> LeaveOrRemoveFromSubscriptionAsync(Guid currentUserId, Guid activeSubId, Guid userIdToRemove, CancellationToken ct = default);
    Task<bool> CancelSubscriptionAsync(Guid currentUserId, Guid activeSubId, CancellationToken ct = default);

}

