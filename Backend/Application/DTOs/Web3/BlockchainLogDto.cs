namespace Application.DTOs.Web3;

public record BlockchainLogDto(
    Guid Id,
    string RawUserId,
    Guid? ResolvedUserId,
    string? ResolvedUsername,
    byte PlanType,
    string Buyer,
    long BlockNumber,
    string Status,
    string? ErrorMessage,
    DateTime ProcessedAt);

public record ManualActivateSubscriptionDto(Guid UserId, byte PlanType);