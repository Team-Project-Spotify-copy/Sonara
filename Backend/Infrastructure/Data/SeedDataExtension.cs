using System;
using Domain.Entities.Music;
using Domain.Entities.Playlists;
using Domain.Entities.Podcasts;
using Domain.Entities.Social;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public static class SeedDataExtension
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        // 1. Independent Entities: Roles, Plans, Genres
        var roleUserGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var roleAdminGuid = Guid.Parse("11111111-1111-1111-1111-222222222222");

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = roleUserGuid, Name = "User" },
            new Role { Id = roleAdminGuid, Name = "Admin" }
        );

        var planFreeGuid = Guid.Parse("22222222-2222-2222-2222-111111111111");
        var planIndividualGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var planDuoGuid = Guid.Parse("22222222-2222-2222-2222-333333333333");
        var planFamilyGuid = Guid.Parse("22222222-2222-2222-2222-444444444444");

        modelBuilder.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan
            {
                Id = planFreeGuid,
                Name = "Free",
                Price = 0.00m,
                MaxSlots = 1,
                Features = "Ads included, Audio standard quality, Shuffle only"
            },
            new SubscriptionPlan
            {
                Id = planIndividualGuid,
                Name = "Individual",
                Price = 3.99m,
                MaxSlots = 1,
                Features = "No ads, High quality audio, Offline downloads"
            },
            new SubscriptionPlan
            {
                Id = planDuoGuid,
                Name = "Duo",
                Price = 6.99m,
                MaxSlots = 2,
                Features = "Up to 2 accounts, Explicit filter, Shared mix"
            },
            new SubscriptionPlan
            {
                Id = planFamilyGuid,
                Name = "Family",
                Price = 9.99m,
                MaxSlots = 6,
                Features = "Up to 6 accounts, Explicit filter, Shared mix"
            }
        );

        var genreGuids = new[]
        {
            Guid.Parse("88888888-8888-8888-8888-111111111111"),
            Guid.Parse("88888888-8888-8888-8888-222222222222"),
            Guid.Parse("88888888-8888-8888-8888-333333333333"),
            Guid.Parse("88888888-8888-8888-8888-444444444444"),
            Guid.Parse("88888888-8888-8888-8888-555555555555")
        };

        modelBuilder.Entity<Genre>().HasData(
            new Genre { Id = genreGuids[0], Name = "Synthwave" },
            new Genre { Id = genreGuids[1], Name = "Ambient" },
            new Genre { Id = genreGuids[2], Name = "Classical" },
            new Genre { Id = genreGuids[3], Name = "Cyberpunk" },
            new Genre { Id = genreGuids[4], Name = "Acoustic" }
        );

        // 2. Users
        var userGuids = new[]
        {
            Guid.Parse("33333333-3333-3333-3333-111111111111"),
            Guid.Parse("33333333-3333-3333-3333-222222222222"),
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Guid.Parse("33333333-3333-3333-3333-444444444444"),
            Guid.Parse("33333333-3333-3333-3333-555555555555")
        };

        var activeSubDuoGuid = Guid.Parse("33333333-3333-3333-9999-111111111111");
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var defaultPasswordHash = "AQAAAAIAAYagAAAAEJrO2K4+14xWd2zC2E9vE9xN5/6+7y8z9A0B1C2D3E4F5G6H7I8J9K0L1M2N3O4P==";

        modelBuilder.Entity<User>().HasData(
            new User { Id = userGuids[0], Username = "john_doe", AvatarUrl = "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_1.jpg", Email = "john@example.com", PasswordHash = defaultPasswordHash, RoleId = roleAdminGuid, ActiveSubscriptionId = activeSubDuoGuid, CreatedAt = seedDate },
            new User { Id = userGuids[1], Username = "alex_sound", AvatarUrl = "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_2.jpg", Email = "alex@example.com", PasswordHash = defaultPasswordHash, RoleId = roleUserGuid, ActiveSubscriptionId = null, CreatedAt = seedDate },
            new User { Id = userGuids[2], Username = "maria_keys", AvatarUrl = "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_3.jpg", Email = "maria@example.com", PasswordHash = defaultPasswordHash, RoleId = roleUserGuid, ActiveSubscriptionId = null, CreatedAt = seedDate },
            new User { Id = userGuids[3], Username = "listener99", AvatarUrl = "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_4.jpg", Email = "listener@example.com", PasswordHash = defaultPasswordHash, RoleId = roleUserGuid, ActiveSubscriptionId = null, CreatedAt = seedDate },
            new User { Id = userGuids[4], Username = "podcast_host", AvatarUrl = "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_5.jpg", Email = "podcaster@example.com", PasswordHash = defaultPasswordHash, RoleId = roleUserGuid, ActiveSubscriptionId = null, CreatedAt = seedDate }
        );

        // 3. UserSubscriptions (depends on User and SubscriptionPlan)
        modelBuilder.Entity<UserSubscription>().HasData(
            new UserSubscription
            {
                Id = activeSubDuoGuid,
                PlanId = planDuoGuid,
                OwnerId = userGuids[0],
                ExpiresAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // 4. RefreshTokens & Sessions
        modelBuilder.Entity<RefreshToken>().HasData(
            new RefreshToken { Id = Guid.Parse("44444444-4444-4444-4444-111111111111"), UserId = userGuids[0], Token = "sample_refresh_token_1", ExpiresAt = seedDate.AddDays(7), CreatedAt = seedDate }
        );

        modelBuilder.Entity<Session>().HasData(
            new Session { Id = Guid.Parse("55555555-5555-5555-5555-111111111111"), UserId = userGuids[0], DeviceName = "Windows PC - Chrome", IpAddress = "192.168.1.10", CreatedAt = seedDate, LastActiveAt = seedDate }
        );

        // 5. Artists
        var artistGuids = new[]
        {
            Guid.Parse("66666666-6666-6666-6666-111111111111"),
            Guid.Parse("66666666-6666-6666-6666-222222222222"),
            Guid.Parse("66666666-6666-6666-6666-333333333333"),
            Guid.Parse("66666666-6666-6666-6666-444444444444"),
            Guid.Parse("66666666-6666-6666-6666-555555555555")
        };

        modelBuilder.Entity<Artist>().HasData(
            new Artist { Id = artistGuids[0], UserId = userGuids[0], Name = "The Midnight Wave", Bio = "Indie synthwave producer from Kyiv.", AvatarUrl = "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_1.jpg", Verified = true },
            new Artist { Id = artistGuids[1], UserId = userGuids[1], Name = "Alex Sound", Bio = "Electronic & Ambient music creator.", AvatarUrl = "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_2.jpg", Verified = true },
            new Artist { Id = artistGuids[2], UserId = userGuids[2], Name = "Maria Keys", Bio = "Classical pianist making modern neo-classical.", AvatarUrl = "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_3.jpg", Verified = true },
            new Artist { Id = artistGuids[3], UserId = userGuids[3], Name = "CyberPulse", Bio = "Industrial Cyberpunk soundscapes.", AvatarUrl = "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_4.jpg", Verified = false },
            new Artist { Id = artistGuids[4], UserId = userGuids[4], Name = "Acoustic Dreams", Bio = "Chill acoustic guitar vibes.", AvatarUrl = "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_5.jpg", Verified = false }
        );

        // 6. Albums
        var albumGuids = new[]
        {
            Guid.Parse("77777777-7777-7777-7777-111111111111"),
            Guid.Parse("77777777-7777-7777-7777-222222222222"),
            Guid.Parse("77777777-7777-7777-7777-333333333333"),
            Guid.Parse("77777777-7777-7777-7777-444444444444"),
            Guid.Parse("77777777-7777-7777-7777-555555555555")
        };

        modelBuilder.Entity<Album>().HasData(
            new Album { Id = albumGuids[0], ArtistId = artistGuids[0], Title = "Neon Nights", Type = "LP", ReleaseDate = new DateTime(2023, 5, 10, 0, 0, 0, DateTimeKind.Utc), CoverUrl = "https://sonarastorage.blob.core.windows.net/images/albums/album_1.jpg", CreatedAt = seedDate },
            new Album { Id = albumGuids[1], ArtistId = artistGuids[1], Title = "Deep Ocean", Type = "EP", ReleaseDate = new DateTime(2023, 8, 15, 0, 0, 0, DateTimeKind.Utc), CoverUrl = "https://sonarastorage.blob.core.windows.net/images/albums/album_2.jpg", CreatedAt = seedDate },
            new Album { Id = albumGuids[2], ArtistId = artistGuids[2], Title = "Piano Echoes", Type = "LP", ReleaseDate = new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Utc), CoverUrl = "https://sonarastorage.blob.core.windows.net/images/albums/album_3.jpg", CreatedAt = seedDate },
            new Album { Id = albumGuids[3], ArtistId = artistGuids[3], Title = "System Crash", Type = "Single", ReleaseDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc), CoverUrl = "https://sonarastorage.blob.core.windows.net/images/albums/album_4.jpg", CreatedAt = seedDate },
            new Album { Id = albumGuids[4], ArtistId = artistGuids[4], Title = "Sunset Chill", Type = "EP", ReleaseDate = new DateTime(2024, 6, 12, 0, 0, 0, DateTimeKind.Utc), CoverUrl = "https://sonarastorage.blob.core.windows.net/images/albums/album_5.jpg", CreatedAt = seedDate }
        );

        // 7. Tracks
        var trackGuids = new[]
        {
            Guid.Parse("99999999-9999-9999-9999-111111111111"),
            Guid.Parse("99999999-9999-9999-9999-222222222222"),
            Guid.Parse("99999999-9999-9999-9999-333333333333"),
            Guid.Parse("99999999-9999-9999-9999-444444444444"),
            Guid.Parse("99999999-9999-9999-9999-555555555555")
        };

        modelBuilder.Entity<Track>().HasData(
            new Track { Id = trackGuids[0], AlbumId = albumGuids[0], ArtistId = artistGuids[0], Title = "Midnight City Drive", DurationMs = 210000, AudioUrl = "https://sonarastorage.blob.core.windows.net/audio/tracks/tracks_1.mp3", PlaysCount = 12500, CreatedAt = seedDate },
            new Track { Id = trackGuids[1], AlbumId = albumGuids[1], ArtistId = artistGuids[1], Title = "Abyssal Silence", DurationMs = 340000, AudioUrl = "https://sonarastorage.blob.core.windows.net/audio/tracks/tracks_2.mp3", PlaysCount = 8400, CreatedAt = seedDate },
            new Track { Id = trackGuids[2], AlbumId = albumGuids[2], ArtistId = artistGuids[2], Title = "Moonlight Sonata Var.", DurationMs = 185000, AudioUrl = "https://sonarastorage.blob.core.windows.net/audio/tracks/tracks_3.mp3", PlaysCount = 3200, CreatedAt = seedDate },
            new Track { Id = trackGuids[3], AlbumId = albumGuids[3], ArtistId = artistGuids[3], Title = "Overdrive", DurationMs = 195000, AudioUrl = "https://sonarastorage.blob.core.windows.net/audio/tracks/tracks_4.mp3", PlaysCount = 45000, CreatedAt = seedDate },
            new Track { Id = trackGuids[4], AlbumId = albumGuids[4], ArtistId = artistGuids[4], Title = "Warm Breeze", DurationMs = 160000, AudioUrl = "https://sonarastorage.blob.core.windows.net/audio/tracks/tracks_5.mp3", PlaysCount = 6100, CreatedAt = seedDate }
        );

        // 8. TrackGenre
        modelBuilder.Entity<TrackGenre>().HasData(
            new TrackGenre { TrackId = trackGuids[0], GenreId = genreGuids[0] },
            new TrackGenre { TrackId = trackGuids[1], GenreId = genreGuids[1] },
            new TrackGenre { TrackId = trackGuids[2], GenreId = genreGuids[2] },
            new TrackGenre { TrackId = trackGuids[3], GenreId = genreGuids[3] },
            new TrackGenre { TrackId = trackGuids[4], GenreId = genreGuids[4] }
        );

        // 9. Playlists & PlaylistTrack
        var playlistGuids = new[]
        {
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-111111111111"),
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-222222222222"),
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-333333333333"),
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-444444444444"),
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-555555555555")
        };

        modelBuilder.Entity<Playlist>().HasData(
            new Playlist { Id = playlistGuids[0], UserId = userGuids[0], Name = "Night Vibes", Description = "Best tracks for late-night coding.", IsPrivate = false, CoverUrl = "https://sonarastorage.blob.core.windows.net/images/playlists/playlists_1.jpg", CreatedAt = seedDate },
            new Playlist { Id = playlistGuids[1], UserId = userGuids[1], Name = "Focus & Flow", Description = "Ambient background soundscapes.", IsPrivate = false, CoverUrl = "https://sonarastorage.blob.core.windows.net/images/playlists/playlists_2.jpg", CreatedAt = seedDate },
            new Playlist { Id = playlistGuids[2], UserId = userGuids[2], Name = "Classical Chill", Description = "Peaceful piano compositions.", IsPrivate = true, CoverUrl = "https://sonarastorage.blob.core.windows.net/images/playlists/playlists_3.jpg", CreatedAt = seedDate },
            new Playlist { Id = playlistGuids[3], UserId = userGuids[3], Name = "Cyberpunk Mix", Description = "Fast & aggressive synth sounds.", IsPrivate = false, CoverUrl = "https://sonarastorage.blob.core.windows.net/images/playlists/playlists_4.jpg", CreatedAt = seedDate },
            new Playlist { Id = playlistGuids[4], UserId = userGuids[4], Name = "Acoustic Favorites", Description = "Soft guitar tracks.", IsPrivate = false, CoverUrl = "https://sonarastorage.blob.core.windows.net/images/playlists/playlists_5.jpg", CreatedAt = seedDate }
        );

        modelBuilder.Entity<PlaylistTrack>().HasData(
            new PlaylistTrack { PlaylistId = playlistGuids[0], TrackId = trackGuids[0], AddedAt = seedDate },
            new PlaylistTrack { PlaylistId = playlistGuids[1], TrackId = trackGuids[1], AddedAt = seedDate },
            new PlaylistTrack { PlaylistId = playlistGuids[2], TrackId = trackGuids[2], AddedAt = seedDate },
            new PlaylistTrack { PlaylistId = playlistGuids[3], TrackId = trackGuids[3], AddedAt = seedDate },
            new PlaylistTrack { PlaylistId = playlistGuids[4], TrackId = trackGuids[4], AddedAt = seedDate }
        );

        // 10. LikedTrack & ListeningHistory
        modelBuilder.Entity<LikedTrack>().HasData(
            new LikedTrack { UserId = userGuids[0], TrackId = trackGuids[0], LikedAt = seedDate },
            new LikedTrack { UserId = userGuids[1], TrackId = trackGuids[1], LikedAt = seedDate },
            new LikedTrack { UserId = userGuids[2], TrackId = trackGuids[2], LikedAt = seedDate },
            new LikedTrack { UserId = userGuids[3], TrackId = trackGuids[3], LikedAt = seedDate },
            new LikedTrack { UserId = userGuids[4], TrackId = trackGuids[4], LikedAt = seedDate }
        );

        modelBuilder.Entity<ListeningHistory>().HasData(
            new ListeningHistory { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-111111111111"), UserId = userGuids[0], TrackId = trackGuids[0], ListenedAt = seedDate, DurationListenedMs = 210000 },
            new ListeningHistory { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-222222222222"), UserId = userGuids[1], TrackId = trackGuids[1], ListenedAt = seedDate, DurationListenedMs = 120000 },
            new ListeningHistory { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-333333333333"), UserId = userGuids[2], TrackId = trackGuids[2], ListenedAt = seedDate, DurationListenedMs = 185000 },
            new ListeningHistory { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-444444444444"), UserId = userGuids[3], TrackId = trackGuids[3], ListenedAt = seedDate, DurationListenedMs = 90000 },
            new ListeningHistory { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-555555555555"), UserId = userGuids[4], TrackId = trackGuids[4], ListenedAt = seedDate, DurationListenedMs = 160000 }
        );

        // 11. Podcasts & PodcastEpisodes
        var podcastGuids = new[]
        {
            Guid.Parse("cccccccc-cccc-cccc-cccc-111111111111"),
            Guid.Parse("cccccccc-cccc-cccc-cccc-222222222222"),
            Guid.Parse("cccccccc-cccc-cccc-cccc-333333333333"),
            Guid.Parse("cccccccc-cccc-cccc-cccc-444444444444"),
            Guid.Parse("cccccccc-cccc-cccc-cccc-555555555555")
        };

        modelBuilder.Entity<Podcast>().HasData(
            new Podcast { Id = podcastGuids[0], AuthorId = userGuids[4], Title = "Tech Talk Daily", Description = "Latest insights into software engineering.", CoverUrl = "https://sonarastorage.blob.core.windows.net/images/podcasts/podcasts_1.jpg" },
            new Podcast { Id = podcastGuids[1], AuthorId = userGuids[0], Title = "Music Production 101", Description = "How to mix and master your tracks.", CoverUrl = "https://sonarastorage.blob.core.windows.net/images/podcasts/podcasts_2.jpg" },
            new Podcast { Id = podcastGuids[2], AuthorId = userGuids[1], Title = "Ambient Life", Description = "Discussions on sound design and field recording.", CoverUrl = "https://sonarastorage.blob.core.windows.net/images/podcasts/podcasts_3.jpg" },
            new Podcast { Id = podcastGuids[3], AuthorId = userGuids[2], Title = "Classical History", Description = "Deep dives into legendary composers.", CoverUrl = "https://sonarastorage.blob.core.windows.net/images/podcasts/podcasts_4.jpg" },
            new Podcast { Id = podcastGuids[4], AuthorId = userGuids[3], Title = "Cyber Culture", Description = "Sci-fi movies, music, and gaming.", CoverUrl = "https://sonarastorage.blob.core.windows.net/images/podcasts/podcasts_5.jpg" }
        );

        modelBuilder.Entity<PodcastEpisode>().HasData(
            new PodcastEpisode { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-111111111111"), PodcastId = podcastGuids[0], Title = "Ep 1: C# and .NET Features", AudioUrl = "https://sonarastorage.blob.core.windows.net/audio/podcasts/podcasts_1.mp3", DurationMs = 1800000, ReleaseDate = seedDate },
            new PodcastEpisode { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-222222222222"), PodcastId = podcastGuids[1], Title = "Ep 1: Choosing Your First DAW", AudioUrl = "https://sonarastorage.blob.core.windows.net/audio/podcasts/podcasts_2.mp3", DurationMs = 2400000, ReleaseDate = seedDate },
            new PodcastEpisode { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-333333333333"), PodcastId = podcastGuids[2], Title = "Ep 1: The Magic of Reverb", AudioUrl = "https://sonarastorage.blob.core.windows.net/audio/podcasts/podcasts_3.mp3", DurationMs = 1500000, ReleaseDate = seedDate },
            new PodcastEpisode { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-444444444444"), PodcastId = podcastGuids[3], Title = "Ep 1: Chopin’s Nocturnes", AudioUrl = "https://sonarastorage.blob.core.windows.net/audio/podcasts/podcasts_4.mp3", DurationMs = 2100000, ReleaseDate = seedDate },
            new PodcastEpisode { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-555555555555"), PodcastId = podcastGuids[4], Title = "Ep 1: Retrofuturism in Games", AudioUrl = "https://sonarastorage.blob.core.windows.net/audio/podcasts/podcasts_5.mp3", DurationMs = 2700000, ReleaseDate = seedDate }
        );

        // 12. Followers
        modelBuilder.Entity<Follower>().HasData(
            new Follower { FollowerId = userGuids[1], FollowedId = userGuids[0], CreatedAt = seedDate },
            new Follower { FollowerId = userGuids[2], FollowedId = userGuids[0], CreatedAt = seedDate },
            new Follower { FollowerId = userGuids[3], FollowedId = userGuids[1], CreatedAt = seedDate },
            new Follower { FollowerId = userGuids[4], FollowedId = userGuids[2], CreatedAt = seedDate },
            new Follower { FollowerId = userGuids[0], FollowedId = userGuids[4], CreatedAt = seedDate }
        );

        // 13. ListeningRooms & RoomMembers
        var roomGuids = new[]
        {
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-111111111111"),
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-222222222222"),
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-333333333333"),
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-444444444444"),
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-555555555555")
        };

        modelBuilder.Entity<ListeningRoom>().HasData(
            new ListeningRoom { Id = roomGuids[0], HostId = userGuids[0], Name = "Synth Party", CurrentTrackId = trackGuids[0], IsActive = true, CreatedAt = seedDate },
            new ListeningRoom { Id = roomGuids[1], HostId = userGuids[1], Name = "Chill Lounge", CurrentTrackId = trackGuids[1], IsActive = true, CreatedAt = seedDate },
            new ListeningRoom { Id = roomGuids[2], HostId = userGuids[2], Name = "Piano Study Group", CurrentTrackId = trackGuids[2], IsActive = false, CreatedAt = seedDate },
            new ListeningRoom { Id = roomGuids[3], HostId = userGuids[3], Name = "Cyber Wave", CurrentTrackId = trackGuids[3], IsActive = true, CreatedAt = seedDate },
            new ListeningRoom { Id = roomGuids[4], HostId = userGuids[4], Name = "Acoustic Hour", CreatedAt = seedDate, IsActive = false, CurrentTrackId = trackGuids[4] }
        );

        modelBuilder.Entity<RoomMember>().HasData(
            new RoomMember { RoomId = roomGuids[0], UserId = userGuids[1], JoinedAt = seedDate },
            new RoomMember { RoomId = roomGuids[1], UserId = userGuids[2], JoinedAt = seedDate },
            new RoomMember { RoomId = roomGuids[2], UserId = userGuids[3], JoinedAt = seedDate },
            new RoomMember { RoomId = roomGuids[3], UserId = userGuids[4], JoinedAt = seedDate },
            new RoomMember { RoomId = roomGuids[4], UserId = userGuids[0], JoinedAt = seedDate }
        );
    }
}
