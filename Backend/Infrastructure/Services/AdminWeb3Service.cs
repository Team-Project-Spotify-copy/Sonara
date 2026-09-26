namespace Infrastructure.Services;

using Application.DTOs.Web3;
using Application.Interfaces.Services;
using Domain.Entities.Web3;
using Microsoft.EntityFrameworkCore;

public class AdminWeb3Service : IAdminWeb3Service
{
    private readonly SonaraDbContext _db;
    private readonly ISubscriptionService _subscriptionService;

    public AdminWeb3Service(SonaraDbContext db, ISubscriptionService subscriptionService)
    {
        _db = db;
        _subscriptionService = subscriptionService;
    }

    public async Task<IReadOnlyList<BlockchainLogDto>> GetLogsAsync(CancellationToken ct = default)
    {
        return await _db.BlockchainTransactionLogs
            .AsNoTracking()
            .OrderByDescending(l => l.ProcessedAt)
            .Take(200)
            .Select(l => new BlockchainLogDto(
                l.Id,
                l.RawUserId,
                l.ResolvedUserId,
                l.ResolvedUser != null ? l.ResolvedUser.Username : null,
                l.PlanType,
                l.Buyer,
                l.BlockNumber,
                l.Status.ToString(),
                l.ErrorMessage,
                l.ProcessedAt))
            .ToListAsync(ct);
    }

    public async Task ManuallyActivateSubscriptionAsync(Guid userId, byte planType, CancellationToken ct = default)
    {
        await _subscriptionService.ProcessBlockchainPurchaseAsync(userId, planType, ct);

        _db.BlockchainTransactionLogs.Add(new BlockchainTransactionLog
        {
            RawUserId = userId.ToString(),
            ResolvedUserId = userId,
            PlanType = planType,
            Buyer = "manual-admin-action",
            BlockNumber = 0,
            Status = BlockchainEventStatus.Processed,
            ErrorMessage = "Активовано вручну через адмін-панель (не через блокчейн).",
            ProcessedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }
}