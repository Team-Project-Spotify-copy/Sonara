namespace Application.Interfaces.Services;

using Application.DTOs.Music;
using Application.Helpers;

public enum TrackSortOrder
{
    Newest = 0,
    Popular = 1,
    Title = 2
}

public record TrackQuery(
    int Page = 1,
    int PageSize = 20,
    string? Genre = null,
    Guid? ArtistId = null,
    Guid? AlbumId = null,
    TrackSortOrder Sort = TrackSortOrder.Newest);

public interface IMusicCatalogService
{
    Task<PaginatedList<TrackDto>> GetTracksAsync(TrackQuery query, Guid? currentUserId, CancellationToken ct = default);

    Task<TrackDetailsDto> GetTrackByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default);

    Task<IReadOnlyList<TrackDto>> GetTracksByIdsAsync(IReadOnlyCollection<Guid> ids, Guid? currentUserId, CancellationToken ct = default);

    Task<AlbumDto> GetAlbumByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default);

    Task<ArtistDto> GetArtistByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default);

    Task<SearchResponseDto> SearchAsync(string? query, int limit, Guid? currentUserId, CancellationToken ct = default);

    Task<IReadOnlyList<TrackDto>> GetPopularTracksAsync(int count, Guid? currentUserId, CancellationToken ct = default);

    Task<IReadOnlyList<AlbumSummaryDto>> GetPopularAlbumsAsync(int count, CancellationToken ct = default);
}
