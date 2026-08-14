using Application.DTOs.Music;
using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Services;

namespace Infrastructure.Services;

public class CachedMusicCatalogService : IMusicCatalogService
{
    private static readonly TimeSpan PopularListsTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan SearchTtl = TimeSpan.FromMinutes(3);

    private readonly MusicCatalogService _inner;
    private readonly ICacheService _cache;

    public CachedMusicCatalogService(MusicCatalogService inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public Task<PaginatedList<TrackDto>> GetTracksAsync(TrackQuery query, Guid? currentUserId, CancellationToken ct = default)
        => _inner.GetTracksAsync(query, currentUserId, ct);

    public Task<TrackDetailsDto> GetTrackByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default)
        => _inner.GetTrackByIdAsync(id, currentUserId, ct);

    public Task<IReadOnlyList<TrackDto>> GetTracksByIdsAsync(IReadOnlyCollection<Guid> ids, Guid? currentUserId, CancellationToken ct = default)
        => _inner.GetTracksByIdsAsync(ids, currentUserId, ct);

    public Task<AlbumDto> GetAlbumByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default)
        => _inner.GetAlbumByIdAsync(id, currentUserId, ct);

    public Task<ArtistDto> GetArtistByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default)
        => _inner.GetArtistByIdAsync(id, currentUserId, ct);

    public async Task<SearchResponseDto> SearchAsync(string? query, int limit, Guid? currentUserId, CancellationToken ct = default)
    {
        if (currentUserId is not null)
        {
            return await _inner.SearchAsync(query, limit, currentUserId, ct);
        }

        var key = $"search:{(query ?? string.Empty).Trim().ToLowerInvariant()}:{limit}";

        var cached = await _cache.GetAsync<SearchResponseDto>(key);
        if (cached is not null) return cached;

        var result = await _inner.SearchAsync(query, limit, currentUserId, ct);
        await _cache.SetAsync(key, result, SearchTtl);
        return result;
    }

    public async Task<IReadOnlyList<TrackDto>> GetPopularTracksAsync(int count, Guid? currentUserId, CancellationToken ct = default)
    {
        if (currentUserId is not null)
        {
            return await _inner.GetPopularTracksAsync(count, currentUserId, ct);
        }

        var key = $"tracks:popular:{count}";

        var cached = await _cache.GetAsync<List<TrackDto>>(key);
        if (cached is not null) return cached;

        var result = await _inner.GetPopularTracksAsync(count, currentUserId, ct);
        await _cache.SetAsync(key, result.ToList(), PopularListsTtl);
        return result;
    }

    public async Task<IReadOnlyList<AlbumSummaryDto>> GetPopularAlbumsAsync(int count, CancellationToken ct = default)
    {
        var key = $"albums:popular:{count}";

        var cached = await _cache.GetAsync<List<AlbumSummaryDto>>(key);
        if (cached is not null) return cached;

        var result = await _inner.GetPopularAlbumsAsync(count, ct);
        await _cache.SetAsync(key, result.ToList(), PopularListsTtl);
        return result;
    }
}
