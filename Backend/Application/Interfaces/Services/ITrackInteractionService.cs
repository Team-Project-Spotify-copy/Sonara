using Application.DTOs.Music;
using Application.Helpers;

namespace Application.Interfaces.Services;

public interface ITrackInteractionService
{
    Task<TrackLikeStateDto> GetLikeStateAsync(Guid trackId, Guid userId, CancellationToken ct = default);

    Task<TrackLikeStateDto> LikeAsync(Guid trackId, Guid userId, CancellationToken ct = default);

    Task<TrackLikeStateDto> UnlikeAsync(Guid trackId, Guid userId, CancellationToken ct = default);

    Task<PaginatedList<TrackDto>> GetLikedTracksAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    Task<ListenRegistrationDto> RegisterListenAsync(Guid trackId, Guid userId, int? durationListenedMs, CancellationToken ct = default);

    Task<PaginatedList<ListeningHistoryEntryDto>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}
