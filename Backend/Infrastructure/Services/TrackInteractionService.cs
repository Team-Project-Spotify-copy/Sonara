using Application.Entities.Music;
using Application.Entities.Playlists;
using Application.Exceptions;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class TrackInteractionService : ITrackInteractionService
{
    private readonly SonaraDbContext _db;

    public TrackInteractionService(SonaraDbContext db)
    {
        _db = db;
    }

    public async Task LikeAsync(Guid trackId, Guid userId, CancellationToken ct = default)
    {
        await EnsureTrackExistsAsync(trackId, ct);

        var alreadyLiked = await _db.LikedTracks
            .AnyAsync(l => l.TrackId == trackId && l.UserId == userId, ct);

        if (alreadyLiked)
        {
            return;
        }

        _db.LikedTracks.Add(new LikedTrack
        {
            TrackId = trackId,
            UserId = userId,
            LikedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task UnlikeAsync(Guid trackId, Guid userId, CancellationToken ct = default)
    {
        var like = await _db.LikedTracks
            .FirstOrDefaultAsync(l => l.TrackId == trackId && l.UserId == userId, ct);

        if (like is null)
        {
            return;
        }

        _db.LikedTracks.Remove(like);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RegisterListenAsync(Guid trackId, Guid userId, int? durationListenedMs, CancellationToken ct = default)
    {
        var track = await _db.Tracks.FirstOrDefaultAsync(t => t.Id == trackId, ct);
        if (track is null)
        {
            throw new NotFoundException(nameof(Track), trackId);
        }

        _db.ListeningHistories.Add(new ListeningHistory
        {
            TrackId = trackId,
            UserId = userId,
            ListenedAt = DateTime.UtcNow,
            DurationListenedMs = durationListenedMs
        });

        track.PlaysCount += 1;

        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureTrackExistsAsync(Guid trackId, CancellationToken ct)
    {
        var exists = await _db.Tracks.AnyAsync(t => t.Id == trackId, ct);
        if (!exists)
        {
            throw new NotFoundException(nameof(Track), trackId);
        }
    }
}