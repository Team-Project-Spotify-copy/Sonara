namespace Application.DTOs.Subscription;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    decimal Price,
    int MaxSlots,
    string Features
);

public record CreateSubscriptionPlanDto(
    string Name,
    decimal Price,
    int MaxSlots,
    string Features
);

public record UpdateSubscriptionPlanDto(
    string Name,
    decimal Price,
    int MaxSlots,
    string Features
);

public record GetSubscriptionPlanDto(
    Guid Id,
    string Name
);

