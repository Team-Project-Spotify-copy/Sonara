using Domain.Entities.Music;
using Domain.Entities.Playlists;
using Domain.Entities.Users;

namespace Sonara.Tests.Infrastructure;

public static class TestData
{
    public static readonly Guid SeededUserRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid SeededFreeSubscriptionId = Guid.Parse("22222222-2222-2222-2222-111111111111");

    public static User NewUser(string username)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = $"{username}@tests.local",
            PasswordHash = "not-a-real-hash",
            RoleId = SeededUserRoleId,
            SubscriptionId = SeededFreeSubscriptionId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Artist NewArtist(Guid userId, string name, bool verified = false)
    {
        return new Artist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Bio = $"{name} bio",
            AvatarUrl = $"https://cdn.test/artists/{name}.jpg",
            Verified = verified
        };
    }

    public static Album NewAlbum(Guid artistId, string title, string? coverUrl = "https://cdn.test/covers/album.jpg")
    {
        return new Album
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            Title = title,
            CoverUrl = coverUrl,
            Type = "Album",
            ReleaseDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Track NewTrack(
        Guid artistId,
        string title,
        Guid? albumId = null,
        int durationMs = 200_000,
        long playsCount = 0,
        string? audioUrl = "https://cdn.test/audio/track.mp3",
        DateTime? createdAt = null)
    {
        return new Track
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            AlbumId = albumId,
            Title = title,
            DurationMs = durationMs,
            AudioUrl = audioUrl ?? string.Empty,
            PlaysCount = playsCount,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
    }

    public static Playlist NewPlaylist(Guid userId, string name, bool isPrivate = false)
    {
        return new Playlist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Description = null,
            IsPrivate = isPrivate,
            CoverUrl = null,
            CreatedAt = DateTime.UtcNow
        };
    }
}
