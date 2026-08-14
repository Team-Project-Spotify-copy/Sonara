using Application.DTOs.Playlists;
using Application.Enums;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities.Music;
using Domain.Entities.Playlists;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PlaylistService : IPlaylistService
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 500;

    private readonly SonaraDbContext _db;
    private readonly IBlobService _blobService;

    public PlaylistService(SonaraDbContext db, IBlobService blobService)
    {
        _db = db;
        _blobService = blobService;
    }

    public async Task<IReadOnlyList<PlaylistDto>> GetMyPlaylistsAsync(Guid currentUserId, CancellationToken ct = default)
    {
        return await _db.Playlists
            .AsNoTracking()
            .Where(p => p.UserId == currentUserId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .Select(PlaylistProjection(currentUserId))
            .ToListAsync(ct);
    }

    public async Task<PlaylistDto> GetByIdAsync(Guid playlistId, Guid? requestingUserId, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureCanView(playlist, requestingUserId);
        return await ProjectAsync(playlistId, requestingUserId, ct);
    }

    public async Task<PlaylistDto> CreateAsync(Guid ownerId, CreatePlaylistRequest request, CancellationToken ct = default)
    {
        var name = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);

        // Обкладинка необовʼязкова: плейліст можна створити з одного поля "назва".
        string? coverUrl = null;
        if (request.CoverImage is { Length: > 0 })
        {
            coverUrl = await _blobService.UploadFileAsync(request.CoverImage, BlobFolder.PlaylistsCovers);
        }

        var playlist = new Playlist
        {
            UserId = ownerId,
            Name = name,
            Description = description,
            IsPrivate = request.IsPrivate,
            CoverUrl = coverUrl,
            CreatedAt = DateTime.UtcNow
        };

        _db.Playlists.Add(playlist);
        await _db.SaveChangesAsync(ct);

        return await ProjectAsync(playlist.Id, ownerId, ct);
    }

    public async Task<PlaylistDto> UpdateAsync(Guid playlistId, Guid ownerId, UpdatePlaylistRequest request, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureIsOwner(playlist, ownerId);

        playlist.Name = NormalizeName(request.Name);
        playlist.Description = NormalizeDescription(request.Description);
        playlist.IsPrivate = request.IsPrivate;

        if (request.CoverImage is { Length: > 0 })
        {
            playlist.CoverUrl = await _blobService.ReplaceFileAsync(
                request.CoverImage,
                playlist.CoverUrl,
                BlobFolder.PlaylistsCovers);
        }

        await _db.SaveChangesAsync(ct);

        return await ProjectAsync(playlistId, ownerId, ct);
    }

    public async Task DeleteAsync(Guid playlistId, Guid ownerId, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureIsOwner(playlist, ownerId);

        if (!string.IsNullOrWhiteSpace(playlist.CoverUrl))
        {
            await _blobService.DeleteFileAsync(playlist.CoverUrl, BlobFolder.PlaylistsCovers);
        }

        _db.Playlists.Remove(playlist);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PlaylistTrackDto>> GetTracksAsync(Guid playlistId, Guid? requestingUserId, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureCanView(playlist, requestingUserId);

        var rows = await _db.PlaylistTracks
            .AsNoTracking()
            .Where(pt => pt.PlaylistId == playlistId)
            .OrderBy(pt => pt.AddedAt)
            .ThenBy(pt => pt.TrackId)
            .Select(CatalogProjections.PlaylistRow(requestingUserId))
            .ToListAsync(ct);

        // Схема не зберігає порядок вручну, тож позиція - це індекс у порядку додавання.
        return rows
            .Select((row, index) => new PlaylistTrackDto(index, row.AddedAt, row.Track))
            .ToList();
    }

    public async Task<PlaylistDto> AddTrackAsync(Guid playlistId, Guid ownerId, Guid trackId, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureIsOwner(playlist, ownerId);

        var trackExists = await _db.Tracks.AnyAsync(t => t.Id == trackId, ct);
        if (!trackExists)
        {
            throw new NotFoundException(nameof(Track), trackId);
        }

        // (PlaylistId, TrackId) - складений первинний ключ, дублікати неможливі за схемою,
        // тому повторне додавання просто не робить нічого.
        var alreadyAdded = await _db.PlaylistTracks
            .AnyAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId, ct);

        if (!alreadyAdded)
        {
            _db.PlaylistTracks.Add(new PlaylistTrack
            {
                PlaylistId = playlistId,
                TrackId = trackId,
                AddedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
        }

        return await ProjectAsync(playlistId, ownerId, ct);
    }

    public async Task<PlaylistDto> RemoveTrackAsync(Guid playlistId, Guid ownerId, Guid trackId, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureIsOwner(playlist, ownerId);

        var link = await _db.PlaylistTracks
            .FirstOrDefaultAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId, ct);

        if (link is not null)
        {
            _db.PlaylistTracks.Remove(link);
            await _db.SaveChangesAsync(ct);
        }

        return await ProjectAsync(playlistId, ownerId, ct);
    }

    private async Task<Playlist> FindOrThrowAsync(Guid playlistId, CancellationToken ct)
    {
        var playlist = await _db.Playlists.FirstOrDefaultAsync(p => p.Id == playlistId, ct);

        if (playlist is null)
        {
            throw new NotFoundException(nameof(Playlist), playlistId);
        }

        return playlist;
    }

    private async Task<PlaylistDto> ProjectAsync(Guid playlistId, Guid? requestingUserId, CancellationToken ct)
    {
        return await _db.Playlists
            .AsNoTracking()
            .Where(p => p.Id == playlistId)
            .Select(PlaylistProjection(requestingUserId))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Playlist), playlistId);
    }

    private static System.Linq.Expressions.Expression<Func<Playlist, PlaylistDto>> PlaylistProjection(Guid? requestingUserId)
    {
        var userId = requestingUserId ?? Guid.Empty;
        var isAuthenticated = requestingUserId.HasValue;

        return p => new PlaylistDto(
            p.Id,
            p.UserId,
            p.User.Username,
            p.Name,
            p.Description,
            p.IsPrivate,
            p.CoverUrl,
            p.CreatedAt,
            p.PlaylistTracks.Count(),
            p.PlaylistTracks.Sum(pt => pt.Track.DurationMs),
            isAuthenticated && p.UserId == userId);
    }

    private static string NormalizeName(string? name)
    {
        var normalized = (name ?? string.Empty).Trim();

        if (normalized.Length == 0)
        {
            throw new ValidationException(nameof(CreatePlaylistRequest.Name), "Playlist name is required.");
        }

        if (normalized.Length > MaxNameLength)
        {
            throw new ValidationException(nameof(CreatePlaylistRequest.Name), $"Playlist name must be at most {MaxNameLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeDescription(string? description)
    {
        var normalized = description?.Trim();

        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        if (normalized.Length > MaxDescriptionLength)
        {
            throw new ValidationException(nameof(CreatePlaylistRequest.Description), $"Description must be at most {MaxDescriptionLength} characters.");
        }

        return normalized;
    }

    private static void EnsureIsOwner(Playlist playlist, Guid userId)
    {
        if (playlist.UserId != userId)
        {
            throw new ForbiddenAccessException("You are not the owner of this playlist.");
        }
    }

    private static void EnsureCanView(Playlist playlist, Guid? requestingUserId)
    {
        if (playlist.IsPrivate && playlist.UserId != requestingUserId)
        {
            throw new ForbiddenAccessException("This playlist is private.");
        }
    }
}
