using Application.DTOs.Subscription;

public record UserSubscriptionDto(
    Guid Id,
    SubscriptionPlanDto Plan,
    Guid OwnerId,
    string OwnerUsername,
    DateTime ExpiresAt,
    IReadOnlyList<UserShortDto> Members
);

public record UserShortDto(
    Guid Id,
    string Username,
    string Email
);