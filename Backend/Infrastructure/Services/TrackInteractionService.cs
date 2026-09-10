using Application.DTOs.Music;
using Application.Exceptions;
using Application.Helpers;
using Application.Interfaces.Services;
using Domain.Entities.Music;
using Domain.Entities.Playlists;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class TrackInteractionService : ITrackInteractionService
{
    public const int MaxPageSize = 100;

    public const int MeaningfulListenMs = 30_000;

    public const int DuplicateWindowSeconds = 30;

    private readonly SonaraDbContext _db;

    public TrackInteractionService(SonaraDbContext db)
    {
        _db = db;
    }

    public async Task<TrackLikeStateDto> GetLikeStateAsync(Guid trackId, Guid userId, CancellationToken ct = default)
    {
        await EnsureTrackExistsAsync(trackId, ct);
        return await BuildLikeStateAsync(trackId, userId, ct);
    }

    public async Task<TrackLikeStateDto> LikeAsync(Guid trackId, Guid userId, CancellationToken ct = default)
    {
        await EnsureTrackExistsAsync(trackId, ct);

        var alreadyLiked = await _db.LikedTracks
            .AnyAsync(l => l.TrackId == trackId && l.UserId == userId, ct);

        if (!alreadyLiked)
        {
            _db.LikedTracks.Add(new LikedTrack
            {
                TrackId = trackId,
                UserId = userId,
                LikedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
        }

        return await BuildLikeStateAsync(trackId, userId, ct);
    }

    public async Task<TrackLikeStateDto> UnlikeAsync(Guid trackId, Guid userId, CancellationToken ct = default)
    {
        await EnsureTrackExistsAsync(trackId, ct);

        var like = await _db.LikedTracks
            .FirstOrDefaultAsync(l => l.TrackId == trackId && l.UserId == userId, ct);

        if (like is not null)
        {
            _db.LikedTracks.Remove(like);
            await _db.SaveChangesAsync(ct);
        }

        return await BuildLikeStateAsync(trackId, userId, ct);
    }

    public Task<PaginatedList<TrackDto>> GetLikedTracksAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.LikedTracks
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.LikedAt)
            .ThenBy(l => l.TrackId)
            .Select(l => l.Track)
            .Select(CatalogProjections.Track(userId));

        return PaginatedList<TrackDto>.CreateAsync(
            query,
            Math.Max(1, page),
            Math.Clamp(pageSize, 1, MaxPageSize),
            ct);
    }

    public async Task<ListenRegistrationDto> RegisterListenAsync(Guid mediaId, Guid userId, int? durationListenedMs, CancellationToken ct = default)
    {
        var track = await _db.Tracks.FirstOrDefaultAsync(t => t.Id == mediaId, ct);

        var episode = track == null ? await _db.PodcastEpisodes.FirstOrDefaultAsync(e => e.Id == mediaId, ct) : null;

        if (track == null && episode == null)
        {
            throw new NotFoundException("MediaItem", mediaId);
        }

        var durationMs = track?.DurationMs ?? episode?.DurationMs ?? 0;
        var currentPlaysCount = track?.PlaysCount ?? 0;

        var required = durationMs > 0
            ? Math.Min(MeaningfulListenMs, Math.Max(1, durationMs / 2))
            : MeaningfulListenMs;

        var listened = Math.Max(0, durationListenedMs ?? 0);

        var result = new ListenRegistrationDto
        {
            TrackId = mediaId,
            PlaysCount = currentPlaysCount,
            RequiredListenedMs = required
        };

        if (listened < required)
        {
            result.Status = ListenRecordStatus.TooShort;
            return result;
        }

        var windowStart = DateTime.UtcNow.AddSeconds(-DuplicateWindowSeconds);
        var recentlyRecorded = await _db.ListeningHistories
            .AnyAsync(h => h.UserId == userId && h.TrackId == mediaId && h.ListenedAt >= windowStart, ct);

        if (recentlyRecorded)
        {
            result.Status = ListenRecordStatus.Throttled;
            return result;
        }

        var listenedAt = DateTime.UtcNow;

        _db.ListeningHistories.Add(new ListeningHistory
        {
            TrackId = mediaId,
            UserId = userId,
            ListenedAt = listenedAt,
            DurationListenedMs = listened
        });

        if (track != null)
        {
            track.PlaysCount += 1;
            result.PlaysCount = track.PlaysCount;
        }

        await _db.SaveChangesAsync(ct);

        result.Status = ListenRecordStatus.Recorded;
        result.ListenedAt = listenedAt;
        return result;
    }

    public Task<PaginatedList<ListeningHistoryEntryDto>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.ListeningHistories
            .AsNoTracking()
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.ListenedAt)
            .ThenBy(h => h.Id)
            .Select(CatalogProjections.HistoryEntry(userId));

        return PaginatedList<ListeningHistoryEntryDto>.CreateAsync(
            query,
            Math.Max(1, page),
            Math.Clamp(pageSize, 1, MaxPageSize),
            ct);
    }

    private async Task<TrackLikeStateDto> BuildLikeStateAsync(Guid trackId, Guid userId, CancellationToken ct)
    {
        var likedAt = await _db.LikedTracks
            .AsNoTracking()
            .Where(l => l.TrackId == trackId && l.UserId == userId)
            .Select(l => (DateTime?)l.LikedAt)
            .FirstOrDefaultAsync(ct);

        var likesCount = await _db.LikedTracks
            .AsNoTracking()
            .LongCountAsync(l => l.TrackId == trackId, ct);

        return new TrackLikeStateDto
        {
            TrackId = trackId,
            IsLiked = likedAt.HasValue,
            LikedAt = likedAt,
            LikesCount = likesCount
        };
    }

    private async Task EnsureTrackExistsAsync(Guid trackId, CancellationToken ct)
    {
        var exists = await _db.Tracks.AnyAsync(t => t.Id == trackId, ct);

        if (!exists)
        {
            exists = await _db.PodcastEpisodes.AnyAsync(e => e.Id == trackId, ct);
        }

        if (!exists)
        {
            throw new NotFoundException("MediaItem", trackId);
        }
    }
}
