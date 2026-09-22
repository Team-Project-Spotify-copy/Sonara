namespace Infrastructure.Services;

using System.Linq.Expressions;
using Application.DTOs.Music;
using Domain.Entities.Music;
using Domain.Entities.Playlists;

internal static class CatalogProjections
{   
    public static Expression<Func<Track, TrackDto>> Track(Guid? currentUserId)
    {
        var isAuthenticated = currentUserId.HasValue;
        var userId = currentUserId ?? Guid.Empty;

        return t => new TrackDto
        {
            Id = t.Id,
            Title = t.Title,
            ArtistId = t.ArtistId,
            ArtistName = t.Artist.Name,
            AlbumId = t.AlbumId,
            AlbumTitle = t.Album != null ? t.Album.Title : null,
            ArtworkUrl = t.Album != null && t.Album.CoverUrl != null ? t.Album.CoverUrl : t.Artist.AvatarUrl,
            DurationMs = t.DurationMs,
            Genres = t.TrackGenres.Select(tg => tg.Genre.Name).ToList(),
            PlaysCount = t.PlaysCount,
            HasStream = !string.IsNullOrEmpty(t.AudioUrl),
            IsLiked = isAuthenticated && t.LikedByUsers.Any(l => l.UserId == userId),
            CreatedAt = t.CreatedAt
        };
    }

    public static Expression<Func<Track, TrackDetailsDto>> TrackDetails(Guid? currentUserId)
    {
        var isAuthenticated = currentUserId.HasValue;
        var userId = currentUserId ?? Guid.Empty;

        return t => new TrackDetailsDto
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
            HasStream = !string.IsNullOrEmpty(t.AudioUrl),
            IsLiked = isAuthenticated && t.LikedByUsers.Any(l => l.UserId == userId),
            CreatedAt = t.CreatedAt
        };
    }

    public static Expression<Func<ListeningHistory, ListeningHistoryEntryDto>> HistoryEntry(Guid userId) =>
        h => new ListeningHistoryEntryDto
        {
            Id = h.Id,
            ListenedAt = h.ListenedAt,
            DurationListenedMs = h.DurationListenedMs,
        };

    public static Expression<Func<PlaylistTrack, PlaylistTrackRow>> PlaylistRow(Guid? currentUserId)
    {
        var trackMapper = Track(currentUserId).Compile();

        return pt => new PlaylistTrackRow
        {
            AddedAt = pt.AddedAt,
        };
    }
}

internal sealed class PlaylistTrackRow
{
    public DateTime AddedAt { get; set; }
    public TrackDto Track { get; set; } = new();
}
