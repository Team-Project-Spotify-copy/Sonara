using Domain.Entities.Users;

namespace Domain.Entities.Web3;

public enum BlockchainEventStatus
{
    Processed = 0,
    UserNotFound = 1,
    Failed = 2
}

public class BlockchainTransactionLog : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string RawUserId { get; set; } = string.Empty;
    public Guid? ResolvedUserId { get; set; }

    public byte PlanType { get; set; }
    public string Buyer { get; set; } = string.Empty;
    public long BlockNumber { get; set; }

    public BlockchainEventStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    public virtual User? ResolvedUser { get; set; }
}