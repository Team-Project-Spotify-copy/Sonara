namespace Infrastructure.Services;

using System.Linq.Expressions;
using Application.DTOs.Music;
using Domain.Entities.Music;
using Domain.Entities.Playlists;

/// <summary>
/// Канонічні проєкції каталогу в DTO. Виконуються повністю на боці БД, тому списки треків
/// ніколи не тягнуть графи сутностей і не спричиняють N+1.
/// </summary>
internal static class CatalogProjections
{
    /// <summary>
    /// Проєкція треку для будь-якого списку. Ознака автентифікації - окремий параметр,
    /// щоб EXISTS для "вподобано" не рахувався для анонімних запитів.
    /// </summary>
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
            HasStream = t.AudioUrl != null && t.AudioUrl != "",
            IsLiked = isAuthenticated && t.LikedByUsers.Any(l => l.UserId == userId),
            CreatedAt = t.CreatedAt
        };
    }

    /// <summary>
    /// Те саме, що <see cref="Track"/>, але для вкладеного використання: LINQ-дерева не можна
    /// підставляти одне в одне без власного відвідувача, тому тіло тут повторюється свідомо.
    /// Тримайте обидві проєкції синхронними.
    /// </summary>
    public static Expression<Func<ListeningHistory, ListeningHistoryEntryDto>> HistoryEntry(Guid userId) =>
        h => new ListeningHistoryEntryDto
        {
            Id = h.Id,
            ListenedAt = h.ListenedAt,
            DurationListenedMs = h.DurationListenedMs,
            Track = new TrackDto
            {
                Id = h.Track.Id,
                Title = h.Track.Title,
                ArtistId = h.Track.ArtistId,
                ArtistName = h.Track.Artist.Name,
                AlbumId = h.Track.AlbumId,
                AlbumTitle = h.Track.Album != null ? h.Track.Album.Title : null,
                ArtworkUrl = h.Track.Album != null && h.Track.Album.CoverUrl != null
                    ? h.Track.Album.CoverUrl
                    : h.Track.Artist.AvatarUrl,
                DurationMs = h.Track.DurationMs,
                Genres = h.Track.TrackGenres.Select(tg => tg.Genre.Name).ToList(),
                PlaysCount = h.Track.PlaysCount,
                HasStream = h.Track.AudioUrl != null && h.Track.AudioUrl != "",
                IsLiked = h.Track.LikedByUsers.Any(l => l.UserId == userId),
                CreatedAt = h.Track.CreatedAt
            }
        };

    /// <summary>
    /// Рядок плейліста. Position не має відповідника в схемі, тому проставляється
    /// після матеріалізації за порядком додавання.
    /// </summary>
    public static Expression<Func<PlaylistTrack, PlaylistTrackRow>> PlaylistRow(Guid? currentUserId)
    {
        var isAuthenticated = currentUserId.HasValue;
        var userId = currentUserId ?? Guid.Empty;

        return pt => new PlaylistTrackRow
        {
            AddedAt = pt.AddedAt,
            Track = new TrackDto
            {
                Id = pt.Track.Id,
                Title = pt.Track.Title,
                ArtistId = pt.Track.ArtistId,
                ArtistName = pt.Track.Artist.Name,
                AlbumId = pt.Track.AlbumId,
                AlbumTitle = pt.Track.Album != null ? pt.Track.Album.Title : null,
                ArtworkUrl = pt.Track.Album != null && pt.Track.Album.CoverUrl != null
                    ? pt.Track.Album.CoverUrl
                    : pt.Track.Artist.AvatarUrl,
                DurationMs = pt.Track.DurationMs,
                Genres = pt.Track.TrackGenres.Select(tg => tg.Genre.Name).ToList(),
                PlaysCount = pt.Track.PlaysCount,
                HasStream = pt.Track.AudioUrl != null && pt.Track.AudioUrl != "",
                IsLiked = isAuthenticated && pt.Track.LikedByUsers.Any(l => l.UserId == userId),
                CreatedAt = pt.Track.CreatedAt
            }
        };
    }
}

internal sealed class PlaylistTrackRow
{
    public DateTime AddedAt { get; set; }
    public TrackDto Track { get; set; } = new();
}
