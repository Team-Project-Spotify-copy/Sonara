namespace Infrastructure.Services;

using System.Linq.Expressions;
using Application.DTOs.Music;
using Application.Exceptions;
using Application.Helpers;
using Application.Interfaces.Services;
using Domain.Entities.Music;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

public class MusicCatalogService : IMusicCatalogService
{
    public const int MaxPageSize = 100;
    public const int MaxSearchLimit = 50;
    public const int MinSearchQueryLength = 2;
    public const int MaxBatchIds = 200;

    private readonly SonaraDbContext _context;

    public MusicCatalogService(SonaraDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TrackDto>> GetTracksAsync(TrackQuery query, Guid? currentUserId, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var tracks = _context.Tracks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Genre))
        {
            var genre = query.Genre.Trim().ToLower();
            tracks = tracks.Where(t => t.TrackGenres.Any(tg => tg.Genre.Name.ToLower() == genre));
        }

        if (query.ArtistId is { } artistId)
        {
            tracks = tracks.Where(t => t.ArtistId == artistId);
        }

        if (query.AlbumId is { } albumId)
        {
            tracks = tracks.Where(t => t.AlbumId == albumId);
        }

        tracks = query.Sort switch
        {
            TrackSortOrder.Popular => tracks.OrderByDescending(t => t.PlaysCount).ThenBy(t => t.Id),
            TrackSortOrder.Title => tracks.OrderBy(t => t.Title).ThenBy(t => t.Id),
            _ => tracks.OrderByDescending(t => t.CreatedAt).ThenBy(t => t.Id)
        };

        return await PaginatedList<TrackDto>.CreateAsync(
            tracks.Select(CatalogProjections.Track(currentUserId)),
            page,
            pageSize,
            ct);
    }

    public async Task<TrackDetailsDto> GetTrackByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default)
    {
        var isAuthenticated = currentUserId.HasValue;
        var userId = currentUserId ?? Guid.Empty;

        var track = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TrackDetailsDto
            {
                Id = t.Id,
                Title = t.Title,
                ArtistId = t.ArtistId,
                ArtistName = t.Artist.Name,
                ArtistAvatarUrl = t.Artist.AvatarUrl,
                ArtistVerified = t.Artist.Verified,
                AlbumId = t.AlbumId,
                AlbumTitle = t.Album != null ? t.Album.Title : null,
                AlbumCoverUrl = t.Album != null ? t.Album.CoverUrl : null,
                AlbumType = t.Album != null ? t.Album.Type : null,
                AlbumReleaseDate = t.Album != null ? t.Album.ReleaseDate : null,
                ArtworkUrl = t.Album != null && t.Album.CoverUrl != null ? t.Album.CoverUrl : t.Artist.AvatarUrl,
                DurationMs = t.DurationMs,
                Genres = t.TrackGenres.Select(tg => tg.Genre.Name).ToList(),
                PlaysCount = t.PlaysCount,
                LikesCount = t.LikedByUsers.Count(),
                HasStream = t.AudioUrl != null && t.AudioUrl != "",
                IsLiked = isAuthenticated && t.LikedByUsers.Any(l => l.UserId == userId),
                CreatedAt = t.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        return track ?? throw new NotFoundException(nameof(Track), id);
    }

    public async Task<IReadOnlyList<TrackDto>> GetTracksByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        Guid? currentUserId,
        CancellationToken ct = default)
    {
        if (ids is null || ids.Count == 0)
        {
            return Array.Empty<TrackDto>();
        }

        if (ids.Count > MaxBatchIds)
        {
            throw new ValidationException("ids", $"At most {MaxBatchIds} track ids can be requested at once.");
        }

        var distinct = ids.Distinct().ToArray();

        var found = await _context.Tracks
            .AsNoTracking()
            .Where(t => distinct.Contains(t.Id))
            .Select(CatalogProjections.Track(currentUserId))
            .ToListAsync(ct);

        var byId = found.ToDictionary(t => t.Id);
        return ids
            .Where(byId.ContainsKey)
            .Select(id => byId[id])
            .ToList();
    }

    public async Task<AlbumDto> GetAlbumByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default)
    {
        var album = await _context.Albums
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AlbumDto
            {
                Id = a.Id,
                Title = a.Title,
                CoverUrl = a.CoverUrl,
                Type = a.Type,
                ReleaseDate = a.ReleaseDate,
                ArtistId = a.ArtistId,
                ArtistName = a.Artist.Name,
                TracksCount = a.Tracks.Count(),
                TotalDurationMs = a.Tracks.Sum(t => t.DurationMs)
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Album), id);

        album.Tracks = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.AlbumId == id)
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Title)
            .Select(CatalogProjections.Track(currentUserId))
            .ToListAsync(ct);

        return album;
    }

    public async Task<ArtistDto> GetArtistByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default)
    {
        var artist = await _context.Artists
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new ArtistDto
            {
                Id = a.Id,
                Name = a.Name,
                Bio = a.Bio,
                AvatarUrl = a.AvatarUrl,
                Verified = a.Verified
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Artist), id);

        artist.Albums = await _context.Albums
            .AsNoTracking()
            .Where(a => a.ArtistId == id)
            .OrderByDescending(a => a.ReleaseDate)
            .Select(AlbumSummaryProjection())
            .ToListAsync(ct);

        artist.TopTracks = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.ArtistId == id)
            .OrderByDescending(t => t.PlaysCount)
            .ThenBy(t => t.Id)
            .Take(10)
            .Select(CatalogProjections.Track(currentUserId))
            .ToListAsync(ct);

        return artist;
    }

    public async Task<SearchResponseDto> SearchAsync(string? query, int limit, Guid? currentUserId, CancellationToken ct = default)
    {
        var normalized = (query ?? string.Empty).Trim();
        var effectiveLimit = Math.Clamp(limit, 1, MaxSearchLimit);

        var response = new SearchResponseDto
        {
            Query = normalized,
            Limit = effectiveLimit
        };

        if (normalized.Length < MinSearchQueryLength)
        {
            return response;
        }

        var term = normalized.ToLower();

        var trackMatches = _context.Tracks.AsNoTracking()
            .Where(t => t.Title.ToLower().Contains(term));

        response.Tracks.Total = await trackMatches.CountAsync(ct);
        response.Tracks.Items = await trackMatches
            .OrderByDescending(t => t.Title.ToLower().StartsWith(term))
            .ThenByDescending(t => t.PlaysCount)
            .ThenBy(t => t.Id)
            .Take(effectiveLimit)
            .Select(CatalogProjections.Track(currentUserId))
            .ToListAsync(ct);

        var artistMatches = _context.Artists.AsNoTracking()
            .Where(a => a.Name.ToLower().Contains(term));

        response.Artists.Total = await artistMatches.CountAsync(ct);
        response.Artists.Items = await artistMatches
            .OrderByDescending(a => a.Name.ToLower().StartsWith(term))
            .ThenByDescending(a => a.Verified)
            .ThenBy(a => a.Id)
            .Take(effectiveLimit)
            .Select(a => new ArtistSummaryDto
            {
                Id = a.Id,
                Name = a.Name,
                AvatarUrl = a.AvatarUrl,
                Verified = a.Verified
            })
            .ToListAsync(ct);

        var albumMatches = _context.Albums.AsNoTracking()
            .Where(a => a.Title.ToLower().Contains(term));

        response.Albums.Total = await albumMatches.CountAsync(ct);
        response.Albums.Items = await albumMatches
            .OrderByDescending(a => a.Title.ToLower().StartsWith(term))
            .ThenByDescending(a => a.ReleaseDate)
            .ThenBy(a => a.Id)
            .Take(effectiveLimit)
            .Select(AlbumSummaryProjection())
            .ToListAsync(ct);

        var playlistMatches = _context.Playlists.AsNoTracking()
            .Where(p => !p.IsPrivate && p.Name.ToLower().Contains(term));

        response.Playlists.Total = await playlistMatches.CountAsync(ct);
        response.Playlists.Items = await playlistMatches
            .OrderByDescending(p => p.Name.ToLower().StartsWith(term))
            .ThenByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .Take(effectiveLimit)
            .Select(p => new PlaylistSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CoverUrl = p.CoverUrl,
                OwnerId = p.UserId,
                OwnerUsername = p.User.Username,
                TracksCount = p.PlaylistTracks.Count()
            })
            .ToListAsync(ct);

        return response;
    }

    public async Task<IReadOnlyList<TrackDto>> GetPopularTracksAsync(
        int count,
        Guid? currentUserId,
        CancellationToken ct = default)
    {
        return await _context.Tracks
            .AsNoTracking()
            .OrderByDescending(t => t.PlaysCount)
            .ThenBy(t => t.Id)
            .Take(Math.Clamp(count, 1, MaxPageSize))
            .Select(CatalogProjections.Track(currentUserId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AlbumSummaryDto>> GetPopularAlbumsAsync(
        int count,
        CancellationToken ct = default)
    {
        return await _context.Albums
            .AsNoTracking()
            .OrderByDescending(a => a.Tracks.Sum(t => (long)t.PlaysCount))
            .ThenBy(a => a.Id)
            .Take(Math.Clamp(count, 1, MaxPageSize))
            .Select(AlbumSummaryProjection())
            .ToListAsync(ct);
    }

    private static Expression<Func<Album, AlbumSummaryDto>> AlbumSummaryProjection() =>
        a => new AlbumSummaryDto
        {
            Id = a.Id,
            Title = a.Title,
            CoverUrl = a.CoverUrl,
            Type = a.Type,
            ReleaseDate = a.ReleaseDate,
            ArtistId = a.ArtistId,
            ArtistName = a.Artist.Name,
            TracksCount = a.Tracks.Count()
        };
}
