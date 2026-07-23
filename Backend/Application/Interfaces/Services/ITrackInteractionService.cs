namespace Application.Interfaces.Services;

public interface ITrackInteractionService
{
    Task LikeAsync(Guid trackId, Guid userId, CancellationToken ct = default);

    Task UnlikeAsync(Guid trackId, Guid userId, CancellationToken ct = default);

    Task RegisterListenAsync(Guid trackId, Guid userId, int? durationListenedMs, CancellationToken ct = default);
}