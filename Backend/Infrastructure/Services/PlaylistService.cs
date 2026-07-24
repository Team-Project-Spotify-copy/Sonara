using Application.DTOs.Playlists;
using Application.Entities.Music;
using Application.Entities.Playlists;
using Application.Exceptions;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PlaylistService : IPlaylistService
{
    private readonly SonaraDbContext _db;

    public PlaylistService(SonaraDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PlaylistDto>> GetMyPlaylistsAsync(Guid currentUserId, CancellationToken ct = default)
    {
        return await _db.Playlists
            .Where(p => p.UserId == currentUserId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PlaylistDto(
                p.Id, p.UserId, p.Name, p.Description, p.IsPrivate, p.CoverUrl, p.CreatedAt, p.PlaylistTracks.Count))
            .ToListAsync(ct);
    }

    public async Task<PlaylistDto> GetByIdAsync(Guid playlistId, Guid? requestingUserId, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureCanView(playlist, requestingUserId);
        return ToDto(playlist);
    }

    public async Task<PlaylistDto> CreateAsync(Guid ownerId, CreatePlaylistRequest request, CancellationToken ct = default)
    {
        var playlist = new Playlist
        {
            UserId = ownerId,
            Name = request.Name,
            Description = request.Description,
            IsPrivate = request.IsPrivate,
            CoverUrl = request.CoverUrl,
            CreatedAt = DateTime.UtcNow
        };

        _db.Playlists.Add(playlist);
        await _db.SaveChangesAsync(ct);
        return ToDto(playlist);
    }

    public async Task<PlaylistDto> UpdateAsync(Guid playlistId, Guid ownerId, UpdatePlaylistRequest request, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureIsOwner(playlist, ownerId);

        playlist.Name = request.Name;
        playlist.Description = request.Description;
        playlist.IsPrivate = request.IsPrivate;
        playlist.CoverUrl = request.CoverUrl;

        await _db.SaveChangesAsync(ct);
        return ToDto(playlist);
    }

    public async Task DeleteAsync(Guid playlistId, Guid ownerId, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureIsOwner(playlist, ownerId);

        _db.Playlists.Remove(playlist);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PlaylistTrackDto>> GetTracksAsync(Guid playlistId, Guid? requestingUserId, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureCanView(playlist, requestingUserId);

        return await _db.PlaylistTracks
            .Where(pt => pt.PlaylistId == playlistId)
            .OrderBy(pt => pt.AddedAt)
            .Include(pt => pt.Track)
            .Select(pt => new PlaylistTrackDto(pt.TrackId, pt.Track.Title, pt.Track.DurationMs, pt.Track.AudioUrl, pt.AddedAt))
            .ToListAsync(ct);
    }

    public async Task AddTrackAsync(Guid playlistId, Guid ownerId, Guid trackId, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureIsOwner(playlist, ownerId);

        var trackExists = await _db.Tracks.AnyAsync(t => t.Id == trackId, ct);
        if (!trackExists)
        {
            throw new NotFoundException(nameof(Track), trackId);
        }

        var alreadyAdded = await _db.PlaylistTracks
            .AnyAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId, ct);

        if (alreadyAdded)
        {
            return;
        }

        _db.PlaylistTracks.Add(new PlaylistTrack
        {
            PlaylistId = playlistId,
            TrackId = trackId,
            AddedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveTrackAsync(Guid playlistId, Guid ownerId, Guid trackId, CancellationToken ct = default)
    {
        var playlist = await FindOrThrowAsync(playlistId, ct);
        EnsureIsOwner(playlist, ownerId);

        var link = await _db.PlaylistTracks
            .FirstOrDefaultAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId, ct);

        if (link is null)
        {
            return;
        }

        _db.PlaylistTracks.Remove(link);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Playlist> FindOrThrowAsync(Guid playlistId, CancellationToken ct)
    {
        var playlist = await _db.Playlists
            .Include(p => p.PlaylistTracks)
            .FirstOrDefaultAsync(p => p.Id == playlistId, ct);

        if (playlist is null)
        {
            throw new NotFoundException(nameof(Playlist), playlistId);
        }

        return playlist;
    }

    private static void EnsureIsOwner(Playlist playlist, Guid userId)
    {
        if (playlist.UserId != userId)
        {
            throw new ForbiddenAccessException("Ви не є власником цього плейлиста.");
        }
    }

    private static void EnsureCanView(Playlist playlist, Guid? requestingUserId)
    {
        if (playlist.IsPrivate && playlist.UserId != requestingUserId)
        {
            throw new ForbiddenAccessException("Цей плейлист приватний.");
        }
    }

    private static PlaylistDto ToDto(Playlist p) => new(
        p.Id, p.UserId, p.Name, p.Description, p.IsPrivate, p.CoverUrl, p.CreatedAt, p.PlaylistTracks?.Count ?? 0);
}