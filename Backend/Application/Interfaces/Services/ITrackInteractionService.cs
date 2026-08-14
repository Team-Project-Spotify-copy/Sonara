using Application.DTOs.Music;
using Application.Helpers;

namespace Application.Interfaces.Services;

public interface ITrackInteractionService
{
    Task<TrackLikeStateDto> GetLikeStateAsync(Guid trackId, Guid userId, CancellationToken ct = default);

    /// <summary>Idempotently adds the track to favourites and returns the resulting state.</summary>
    Task<TrackLikeStateDto> LikeAsync(Guid trackId, Guid userId, CancellationToken ct = default);

    /// <summary>Idempotently removes the track from favourites and returns the resulting state.</summary>
    Task<TrackLikeStateDto> UnlikeAsync(Guid trackId, Guid userId, CancellationToken ct = default);

    Task<PaginatedList<TrackDto>> GetLikedTracksAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Records a MEANINGFUL playback event. Not intended to be called on every progress tick:
    /// short plays and rapid repeats are rejected without writing history.
    /// </summary>
    Task<ListenRegistrationDto> RegisterListenAsync(Guid trackId, Guid userId, int? durationListenedMs, CancellationToken ct = default);

    Task<PaginatedList<ListeningHistoryEntryDto>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}
