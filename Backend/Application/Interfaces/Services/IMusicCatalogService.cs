namespace Application.Interfaces.Services;

using Application.DTOs.Music;
using Application.Helpers;

/// <summary>Sort order for the track catalog.</summary>
public enum TrackSortOrder
{
    Newest = 0,
    Popular = 1,
    Title = 2
}

/// <summary>Track list filters. Values are normalised inside the service.</summary>
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

    /// <summary>Full track payload for the song page. Throws NotFoundException when the track does not exist.</summary>
    Task<TrackDetailsDto> GetTrackByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default);

    /// <summary>
    /// Batch read by id - used to rehydrate a player queue in a single request.
    /// The response preserves the requested order; unknown ids are skipped.
    /// </summary>
    Task<IReadOnlyList<TrackDto>> GetTracksByIdsAsync(IReadOnlyCollection<Guid> ids, Guid? currentUserId, CancellationToken ct = default);

    Task<AlbumDto> GetAlbumByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default);

    Task<ArtistDto> GetArtistByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default);

    Task<SearchResponseDto> SearchAsync(string? query, int limit, Guid? currentUserId, CancellationToken ct = default);
}
