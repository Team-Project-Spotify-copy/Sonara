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

    public Task<PaginatedList<TrackDto>> GetTracksAsync(int pageNumber, int pageSize, string? genre)
        => _inner.GetTracksAsync(pageNumber, pageSize, genre);

    public Task<AlbumDto> GetAlbumByIdAsync(Guid id)
        => _inner.GetAlbumByIdAsync(id);

    public Task<ArtistDto> GetArtistByIdAsync(Guid id)
        => _inner.GetArtistByIdAsync(id);

    public async Task<List<TrackDto>> GetPopularTracksAsync(int count = 20)
    {
        var key = $"tracks:popular:{count}";
        var cached = await _cache.GetAsync<List<TrackDto>>(key);
        if (cached is not null) return cached;

        var result = await _inner.GetPopularTracksAsync(count);
        await _cache.SetAsync(key, result, PopularListsTtl);
        return result;
    }

    public async Task<List<AlbumDto>> GetPopularAlbumsAsync(int count = 20)
    {
        var key = $"albums:popular:{count}";
        var cached = await _cache.GetAsync<List<AlbumDto>>(key);
        if (cached is not null) return cached;

        var result = await _inner.GetPopularAlbumsAsync(count);
        await _cache.SetAsync(key, result, PopularListsTtl);
        return result;
    }

    public async Task<SearchResultDto> SearchAsync(string query, int limit = 10)
    {
        var key = $"search:{query.Trim().ToLowerInvariant()}:{limit}";
        var cached = await _cache.GetAsync<SearchResultDto>(key);
        if (cached is not null) return cached;

        var result = await _inner.SearchAsync(query, limit);
        await _cache.SetAsync(key, result, SearchTtl);
        return result;
    }
}