namespace Application.DTOs.Subscription;

public record SubscriptionDto(
    Guid Id,
    string Name,
    decimal Price,
    string Features
);

public record CreateSubscriptionDto(
    string Name,
    decimal Price,
    string Features
);

public record UpdateSubscriptionDto(
    string Name,
    decimal Price,
    string Features
);

public record GetSubscriptionDto(
    Guid Id,
    string Name
);