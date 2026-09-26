namespace Application.Interfaces.Services;

using Application.DTOs.Web3;

public interface IAdminWeb3Service
{
    Task<IReadOnlyList<BlockchainLogDto>> GetLogsAsync(CancellationToken ct = default);

    Task ManuallyActivateSubscriptionAsync(Guid userId, byte planType, CancellationToken ct = default);
}