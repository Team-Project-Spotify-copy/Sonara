using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace Infrastructure.Migrations
{
    public partial class InitFirtData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Followers_Users_FollowedId",
                table: "Followers");

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("88888888-8888-8888-8888-111111111111"), "Synthwave" },
                    { new Guid("88888888-8888-8888-8888-222222222222"), "Ambient" },
                    { new Guid("88888888-8888-8888-8888-333333333333"), "Classical" },
                    { new Guid("88888888-8888-8888-8888-444444444444"), "Cyberpunk" },
                    { new Guid("88888888-8888-8888-8888-555555555555"), "Acoustic" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "User" },
                    { new Guid("11111111-1111-1111-1111-222222222222"), "Admin" }
                });

            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "Id", "Features", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-111111111111"), "Ads included, Audio standard quality, Shuffle only", "Free", 0.00m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "No ads, High quality audio, Offline downloads", "Premium", 4.99m },
                    { new Guid("22222222-2222-2222-2222-333333333333"), "Up to 6 accounts, Explicit filter, Shared mix", "Family", 7.99m }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedAt", "Email", "PasswordHash", "RoleId", "SubscriptionId", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-111111111111"), "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_1.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "john@example.com", "AQAAAAIAAYagAAAAEJrO2K4+14xWd2zC2E9vE9xN5/6+7y8z9A0B1C2D3E4F5G6H7I8J9K0L1M2N3O4P==", new Guid("11111111-1111-1111-1111-222222222222"), new Guid("22222222-2222-2222-2222-222222222222"), null, "john_doe" },
                    { new Guid("33333333-3333-3333-3333-222222222222"), "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_2.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "alex@example.com", "AQAAAAIAAYagAAAAEJrO2K4+14xWd2zC2E9vE9xN5/6+7y8z9A0B1C2D3E4F5G6H7I8J9K0L1M2N3O4P==", new Guid("11111111-1111-1111-1111-111111111111"), new Guid("22222222-2222-2222-2222-222222222222"), null, "alex_sound" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_3.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "maria@example.com", "AQAAAAIAAYagAAAAEJrO2K4+14xWd2zC2E9vE9xN5/6+7y8z9A0B1C2D3E4F5G6H7I8J9K0L1M2N3O4P==", new Guid("11111111-1111-1111-1111-111111111111"), new Guid("22222222-2222-2222-2222-333333333333"), null, "maria_keys" },
                    { new Guid("33333333-3333-3333-3333-444444444444"), "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_4.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "listener@example.com", "AQAAAAIAAYagAAAAEJrO2K4+14xWd2zC2E9vE9xN5/6+7y8z9A0B1C2D3E4F5G6H7I8J9K0L1M2N3O4P==", new Guid("11111111-1111-1111-1111-111111111111"), new Guid("22222222-2222-2222-2222-111111111111"), null, "listener99" },
                    { new Guid("33333333-3333-3333-3333-555555555555"), "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_5.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "podcaster@example.com", "AQAAAAIAAYagAAAAEJrO2K4+14xWd2zC2E9vE9xN5/6+7y8z9A0B1C2D3E4F5G6H7I8J9K0L1M2N3O4P==", new Guid("11111111-1111-1111-1111-111111111111"), new Guid("22222222-2222-2222-2222-222222222222"), null, "podcast_host" }
                });

            migrationBuilder.InsertData(
                table: "Artists",
                columns: new[] { "Id", "AvatarUrl", "Bio", "Name", "UserId", "Verified" },
                values: new object[,]
                {
                    { new Guid("66666666-6666-6666-6666-111111111111"), "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_1.jpg", "Indie synthwave producer from Kyiv.", "The Midnight Wave", new Guid("33333333-3333-3333-3333-111111111111"), true },
                    { new Guid("66666666-6666-6666-6666-222222222222"), "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_2.jpg", "Electronic & Ambient music creator.", "Alex Sound", new Guid("33333333-3333-3333-3333-222222222222"), true },
                    { new Guid("66666666-6666-6666-6666-333333333333"), "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_3.jpg", "Classical pianist making modern neo-classical.", "Maria Keys", new Guid("33333333-3333-3333-3333-333333333333"), true },
                    { new Guid("66666666-6666-6666-6666-444444444444"), "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_4.jpg", "Industrial Cyberpunk soundscapes.", "CyberPulse", new Guid("33333333-3333-3333-3333-444444444444"), false },
                    { new Guid("66666666-6666-6666-6666-555555555555"), "https://sonarastorage.blob.core.windows.net/images/avatars/avatar_5.jpg", "Chill acoustic guitar vibes.", "Acoustic Dreams", new Guid("33333333-3333-3333-3333-555555555555"), false }
                });

            migrationBuilder.InsertData(
                table: "Followers",
                columns: new[] { "FollowedId", "FollowerId", "CreatedAt" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-555555555555"), new Guid("33333333-3333-3333-3333-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-111111111111"), new Guid("33333333-3333-3333-3333-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-111111111111"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-222222222222"), new Guid("33333333-3333-3333-3333-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("33333333-3333-3333-3333-555555555555"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Playlists",
                columns: new[] { "Id", "CoverUrl", "CreatedAt", "Description", "IsPrivate", "Name", "UserId" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-111111111111"), "https://sonarastorage.blob.core.windows.net/images/playlists/playlists_1.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Best tracks for late-night coding.", false, "Night Vibes", new Guid("33333333-3333-3333-3333-111111111111") },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-222222222222"), "https://sonarastorage.blob.core.windows.net/images/playlists/playlists_2.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ambient background soundscapes.", false, "Focus & Flow", new Guid("33333333-3333-3333-3333-222222222222") },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-333333333333"), "https://sonarastorage.blob.core.windows.net/images/playlists/playlists_3.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Peaceful piano compositions.", true, "Classical Chill", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-444444444444"), "https://sonarastorage.blob.core.windows.net/images/playlists/playlists_4.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fast & aggressive synth sounds.", false, "Cyberpunk Mix", new Guid("33333333-3333-3333-3333-444444444444") },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-555555555555"), "https://sonarastorage.blob.core.windows.net/images/playlists/playlists_5.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Soft guitar tracks.", false, "Acoustic Favorites", new Guid("33333333-3333-3333-3333-555555555555") }
                });

            migrationBuilder.InsertData(
                table: "Podcasts",
                columns: new[] { "Id", "AuthorId", "CoverUrl", "Description", "Title" },
                values: new object[,]
                {
                    { new Guid("cccccccc-cccc-cccc-cccc-111111111111"), new Guid("33333333-3333-3333-3333-555555555555"), "https://sonarastorage.blob.core.windows.net/images/podcasts/podcasts_1.jpg", "Latest insights into software engineering.", "Tech Talk Daily" },
                    { new Guid("cccccccc-cccc-cccc-cccc-222222222222"), new Guid("33333333-3333-3333-3333-111111111111"), "https://sonarastorage.blob.core.windows.net/images/podcasts/podcasts_2.jpg", "How to mix and master your tracks.", "Music Production 101" },
                    { new Guid("cccccccc-cccc-cccc-cccc-333333333333"), new Guid("33333333-3333-3333-3333-222222222222"), "https://sonarastorage.blob.core.windows.net/images/podcasts/podcasts_3.jpg", "Discussions on sound design and field recording.", "Ambient Life" },
                    { new Guid("cccccccc-cccc-cccc-cccc-444444444444"), new Guid("33333333-3333-3333-3333-333333333333"), "https://sonarastorage.blob.core.windows.net/images/podcasts/podcasts_4.jpg", "Deep dives into legendary composers.", "Classical History" },
                    { new Guid("cccccccc-cccc-cccc-cccc-555555555555"), new Guid("33333333-3333-3333-3333-444444444444"), "https://sonarastorage.blob.core.windows.net/images/podcasts/podcasts_5.jpg", "Sci-fi movies, music, and gaming.", "Cyber Culture" }
                });

            migrationBuilder.InsertData(
                table: "RefreshTokens",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "RevokedAt", "Token", "UserId" },
                values: new object[] { new Guid("44444444-4444-4444-4444-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, "sample_refresh_token_1", new Guid("33333333-3333-3333-3333-111111111111") });

            migrationBuilder.InsertData(
                table: "Sessions",
                columns: new[] { "Id", "CreatedAt", "DeviceName", "IpAddress", "LastActiveAt", "UserId" },
                values: new object[] { new Guid("55555555-5555-5555-5555-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Windows PC - Chrome", "192.168.1.10", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-111111111111") });

            migrationBuilder.InsertData(
                table: "Albums",
                columns: new[] { "Id", "ArtistId", "CoverUrl", "CreatedAt", "ReleaseDate", "Title", "Type" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-111111111111"), new Guid("66666666-6666-6666-6666-111111111111"), "https://sonarastorage.blob.core.windows.net/images/albums/album_1.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 5, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Neon Nights", "LP" },
                    { new Guid("77777777-7777-7777-7777-222222222222"), new Guid("66666666-6666-6666-6666-222222222222"), "https://sonarastorage.blob.core.windows.net/images/albums/album_2.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 8, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Deep Ocean", "EP" },
                    { new Guid("77777777-7777-7777-7777-333333333333"), new Guid("66666666-6666-6666-6666-333333333333"), "https://sonarastorage.blob.core.windows.net/images/albums/album_3.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Piano Echoes", "LP" },
                    { new Guid("77777777-7777-7777-7777-444444444444"), new Guid("66666666-6666-6666-6666-444444444444"), "https://sonarastorage.blob.core.windows.net/images/albums/album_4.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System Crash", "Single" },
                    { new Guid("77777777-7777-7777-7777-555555555555"), new Guid("66666666-6666-6666-6666-555555555555"), "https://sonarastorage.blob.core.windows.net/images/albums/album_5.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Sunset Chill", "EP" }
                });

            migrationBuilder.InsertData(
                table: "PodcastEpisodes",
                columns: new[] { "Id", "AudioUrl", "Description", "DurationMs", "PodcastId", "ReleaseDate", "Title" },
                values: new object[,]
                {
                    { new Guid("dddddddd-dddd-dddd-dddd-111111111111"), "https://sonarastorage.blob.core.windows.net/audio/podcasts/podcasts_1.mp3", null, 1800000, new Guid("cccccccc-cccc-cccc-cccc-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ep 1: C# and .NET Features" },
                    { new Guid("dddddddd-dddd-dddd-dddd-222222222222"), "https://sonarastorage.blob.core.windows.net/audio/podcasts/podcasts_2.mp3", null, 2400000, new Guid("cccccccc-cccc-cccc-cccc-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ep 1: Choosing Your First DAW" },
                    { new Guid("dddddddd-dddd-dddd-dddd-333333333333"), "https://sonarastorage.blob.core.windows.net/audio/podcasts/podcasts_3.mp3", null, 1500000, new Guid("cccccccc-cccc-cccc-cccc-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ep 1: The Magic of Reverb" },
                    { new Guid("dddddddd-dddd-dddd-dddd-444444444444"), "https://sonarastorage.blob.core.windows.net/audio/podcasts/podcasts_4.mp3", null, 2100000, new Guid("cccccccc-cccc-cccc-cccc-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ep 1: Chopin’s Nocturnes" },
                    { new Guid("dddddddd-dddd-dddd-dddd-555555555555"), "https://sonarastorage.blob.core.windows.net/audio/podcasts/podcasts_5.mp3", null, 2700000, new Guid("cccccccc-cccc-cccc-cccc-555555555555"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ep 1: Retrofuturism in Games" }
                });

            migrationBuilder.InsertData(
                table: "Tracks",
                columns: new[] { "Id", "AlbumId", "ArtistId", "AudioUrl", "CreatedAt", "DurationMs", "PlaysCount", "Title" },
                values: new object[,]
                {
                    { new Guid("99999999-9999-9999-9999-111111111111"), new Guid("77777777-7777-7777-7777-111111111111"), new Guid("66666666-6666-6666-6666-111111111111"), "https://sonarastorage.blob.core.windows.net/audio/tracks/tracks_1.mp3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 210000, 12500L, "Midnight City Drive" },
                    { new Guid("99999999-9999-9999-9999-222222222222"), new Guid("77777777-7777-7777-7777-222222222222"), new Guid("66666666-6666-6666-6666-222222222222"), "https://sonarastorage.blob.core.windows.net/audio/tracks/tracks_2.mp3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 340000, 8400L, "Abyssal Silence" },
                    { new Guid("99999999-9999-9999-9999-333333333333"), new Guid("77777777-7777-7777-7777-333333333333"), new Guid("66666666-6666-6666-6666-333333333333"), "https://sonarastorage.blob.core.windows.net/audio/tracks/tracks_3.mp3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 185000, 3200L, "Moonlight Sonata Var." },
                    { new Guid("99999999-9999-9999-9999-444444444444"), new Guid("77777777-7777-7777-7777-444444444444"), new Guid("66666666-6666-6666-6666-444444444444"), "https://sonarastorage.blob.core.windows.net/audio/tracks/tracks_4.mp3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 195000, 45000L, "Overdrive" },
                    { new Guid("99999999-9999-9999-9999-555555555555"), new Guid("77777777-7777-7777-7777-555555555555"), new Guid("66666666-6666-6666-6666-555555555555"), "https://sonarastorage.blob.core.windows.net/audio/tracks/tracks_5.mp3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 160000, 6100L, "Warm Breeze" }
                });

            migrationBuilder.InsertData(
                table: "LikedTracks",
                columns: new[] { "TrackId", "UserId", "LikedAt" },
                values: new object[,]
                {
                    { new Guid("99999999-9999-9999-9999-111111111111"), new Guid("33333333-3333-3333-3333-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("99999999-9999-9999-9999-222222222222"), new Guid("33333333-3333-3333-3333-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("99999999-9999-9999-9999-333333333333"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("99999999-9999-9999-9999-444444444444"), new Guid("33333333-3333-3333-3333-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("99999999-9999-9999-9999-555555555555"), new Guid("33333333-3333-3333-3333-555555555555"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "ListeningHistories",
                columns: new[] { "Id", "DurationListenedMs", "ListenedAt", "TrackId", "UserId" },
                values: new object[,]
                {
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-111111111111"), 210000, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("99999999-9999-9999-9999-111111111111"), new Guid("33333333-3333-3333-3333-111111111111") },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-222222222222"), 120000, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("99999999-9999-9999-9999-222222222222"), new Guid("33333333-3333-3333-3333-222222222222") },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-333333333333"), 185000, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("99999999-9999-9999-9999-333333333333"), new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-444444444444"), 90000, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("99999999-9999-9999-9999-444444444444"), new Guid("33333333-3333-3333-3333-444444444444") },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-555555555555"), 160000, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("99999999-9999-9999-9999-555555555555"), new Guid("33333333-3333-3333-3333-555555555555") }
                });

            migrationBuilder.InsertData(
                table: "ListeningRooms",
                columns: new[] { "Id", "CreatedAt", "CurrentTrackId", "HostId", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("eeeeeeee-eeee-eeee-eeee-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("99999999-9999-9999-9999-111111111111"), new Guid("33333333-3333-3333-3333-111111111111"), true, "Synth Party" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("99999999-9999-9999-9999-222222222222"), new Guid("33333333-3333-3333-3333-222222222222"), true, "Chill Lounge" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("99999999-9999-9999-9999-333333333333"), new Guid("33333333-3333-3333-3333-333333333333"), false, "Piano Study Group" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("99999999-9999-9999-9999-444444444444"), new Guid("33333333-3333-3333-3333-444444444444"), true, "Cyber Wave" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-555555555555"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("99999999-9999-9999-9999-555555555555"), new Guid("33333333-3333-3333-3333-555555555555"), false, "Acoustic Hour" }
                });

            migrationBuilder.InsertData(
                table: "PlaylistTracks",
                columns: new[] { "PlaylistId", "TrackId", "AddedAt" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-111111111111"), new Guid("99999999-9999-9999-9999-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-222222222222"), new Guid("99999999-9999-9999-9999-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-333333333333"), new Guid("99999999-9999-9999-9999-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-444444444444"), new Guid("99999999-9999-9999-9999-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-555555555555"), new Guid("99999999-9999-9999-9999-555555555555"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "TrackGenres",
                columns: new[] { "GenreId", "TrackId" },
                values: new object[,]
                {
                    { new Guid("88888888-8888-8888-8888-111111111111"), new Guid("99999999-9999-9999-9999-111111111111") },
                    { new Guid("88888888-8888-8888-8888-222222222222"), new Guid("99999999-9999-9999-9999-222222222222") },
                    { new Guid("88888888-8888-8888-8888-333333333333"), new Guid("99999999-9999-9999-9999-333333333333") },
                    { new Guid("88888888-8888-8888-8888-444444444444"), new Guid("99999999-9999-9999-9999-444444444444") },
                    { new Guid("88888888-8888-8888-8888-555555555555"), new Guid("99999999-9999-9999-9999-555555555555") }
                });

            migrationBuilder.InsertData(
                table: "RoomMembers",
                columns: new[] { "RoomId", "UserId", "JoinedAt" },
                values: new object[,]
                {
                    { new Guid("eeeeeeee-eeee-eeee-eeee-111111111111"), new Guid("33333333-3333-3333-3333-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-222222222222"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-333333333333"), new Guid("33333333-3333-3333-3333-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-444444444444"), new Guid("33333333-3333-3333-3333-555555555555"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-555555555555"), new Guid("33333333-3333-3333-3333-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Followers_Users_FollowedId",
                table: "Followers",
                column: "FollowedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Followers_Users_FollowedId",
                table: "Followers");

            migrationBuilder.DeleteData(
                table: "Followers",
                keyColumns: new[] { "FollowedId", "FollowerId" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-555555555555"), new Guid("33333333-3333-3333-3333-111111111111") });

            migrationBuilder.DeleteData(
                table: "Followers",
                keyColumns: new[] { "FollowedId", "FollowerId" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-111111111111"), new Guid("33333333-3333-3333-3333-222222222222") });

            migrationBuilder.DeleteData(
                table: "Followers",
                keyColumns: new[] { "FollowedId", "FollowerId" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-111111111111"), new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.DeleteData(
                table: "Followers",
                keyColumns: new[] { "FollowedId", "FollowerId" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-222222222222"), new Guid("33333333-3333-3333-3333-444444444444") });

            migrationBuilder.DeleteData(
                table: "Followers",
                keyColumns: new[] { "FollowedId", "FollowerId" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("33333333-3333-3333-3333-555555555555") });

            migrationBuilder.DeleteData(
                table: "LikedTracks",
                keyColumns: new[] { "TrackId", "UserId" },
                keyValues: new object[] { new Guid("99999999-9999-9999-9999-111111111111"), new Guid("33333333-3333-3333-3333-111111111111") });

            migrationBuilder.DeleteData(
                table: "LikedTracks",
                keyColumns: new[] { "TrackId", "UserId" },
                keyValues: new object[] { new Guid("99999999-9999-9999-9999-222222222222"), new Guid("33333333-3333-3333-3333-222222222222") });

            migrationBuilder.DeleteData(
                table: "LikedTracks",
                keyColumns: new[] { "TrackId", "UserId" },
                keyValues: new object[] { new Guid("99999999-9999-9999-9999-333333333333"), new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.DeleteData(
                table: "LikedTracks",
                keyColumns: new[] { "TrackId", "UserId" },
                keyValues: new object[] { new Guid("99999999-9999-9999-9999-444444444444"), new Guid("33333333-3333-3333-3333-444444444444") });

            migrationBuilder.DeleteData(
                table: "LikedTracks",
                keyColumns: new[] { "TrackId", "UserId" },
                keyValues: new object[] { new Guid("99999999-9999-9999-9999-555555555555"), new Guid("33333333-3333-3333-3333-555555555555") });

            migrationBuilder.DeleteData(
                table: "ListeningHistories",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-111111111111"));

            migrationBuilder.DeleteData(
                table: "ListeningHistories",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-222222222222"));

            migrationBuilder.DeleteData(
                table: "ListeningHistories",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-333333333333"));

            migrationBuilder.DeleteData(
                table: "ListeningHistories",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-444444444444"));

            migrationBuilder.DeleteData(
                table: "ListeningHistories",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-555555555555"));

            migrationBuilder.DeleteData(
                table: "PlaylistTracks",
                keyColumns: new[] { "PlaylistId", "TrackId" },
                keyValues: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-111111111111"), new Guid("99999999-9999-9999-9999-111111111111") });

            migrationBuilder.DeleteData(
                table: "PlaylistTracks",
                keyColumns: new[] { "PlaylistId", "TrackId" },
                keyValues: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-222222222222"), new Guid("99999999-9999-9999-9999-222222222222") });

            migrationBuilder.DeleteData(
                table: "PlaylistTracks",
                keyColumns: new[] { "PlaylistId", "TrackId" },
                keyValues: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-333333333333"), new Guid("99999999-9999-9999-9999-333333333333") });

            migrationBuilder.DeleteData(
                table: "PlaylistTracks",
                keyColumns: new[] { "PlaylistId", "TrackId" },
                keyValues: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-444444444444"), new Guid("99999999-9999-9999-9999-444444444444") });

            migrationBuilder.DeleteData(
                table: "PlaylistTracks",
                keyColumns: new[] { "PlaylistId", "TrackId" },
                keyValues: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-555555555555"), new Guid("99999999-9999-9999-9999-555555555555") });

            migrationBuilder.DeleteData(
                table: "PodcastEpisodes",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-111111111111"));

            migrationBuilder.DeleteData(
                table: "PodcastEpisodes",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-222222222222"));

            migrationBuilder.DeleteData(
                table: "PodcastEpisodes",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-333333333333"));

            migrationBuilder.DeleteData(
                table: "PodcastEpisodes",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-444444444444"));

            migrationBuilder.DeleteData(
                table: "PodcastEpisodes",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-555555555555"));

            migrationBuilder.DeleteData(
                table: "RefreshTokens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-111111111111"));

            migrationBuilder.DeleteData(
                table: "RoomMembers",
                keyColumns: new[] { "RoomId", "UserId" },
                keyValues: new object[] { new Guid("eeeeeeee-eeee-eeee-eeee-111111111111"), new Guid("33333333-3333-3333-3333-222222222222") });

            migrationBuilder.DeleteData(
                table: "RoomMembers",
                keyColumns: new[] { "RoomId", "UserId" },
                keyValues: new object[] { new Guid("eeeeeeee-eeee-eeee-eeee-222222222222"), new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.DeleteData(
                table: "RoomMembers",
                keyColumns: new[] { "RoomId", "UserId" },
                keyValues: new object[] { new Guid("eeeeeeee-eeee-eeee-eeee-333333333333"), new Guid("33333333-3333-3333-3333-444444444444") });

            migrationBuilder.DeleteData(
                table: "RoomMembers",
                keyColumns: new[] { "RoomId", "UserId" },
                keyValues: new object[] { new Guid("eeeeeeee-eeee-eeee-eeee-444444444444"), new Guid("33333333-3333-3333-3333-555555555555") });

            migrationBuilder.DeleteData(
                table: "RoomMembers",
                keyColumns: new[] { "RoomId", "UserId" },
                keyValues: new object[] { new Guid("eeeeeeee-eeee-eeee-eeee-555555555555"), new Guid("33333333-3333-3333-3333-111111111111") });

            migrationBuilder.DeleteData(
                table: "Sessions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-111111111111"));

            migrationBuilder.DeleteData(
                table: "TrackGenres",
                keyColumns: new[] { "GenreId", "TrackId" },
                keyValues: new object[] { new Guid("88888888-8888-8888-8888-111111111111"), new Guid("99999999-9999-9999-9999-111111111111") });

            migrationBuilder.DeleteData(
                table: "TrackGenres",
                keyColumns: new[] { "GenreId", "TrackId" },
                keyValues: new object[] { new Guid("88888888-8888-8888-8888-222222222222"), new Guid("99999999-9999-9999-9999-222222222222") });

            migrationBuilder.DeleteData(
                table: "TrackGenres",
                keyColumns: new[] { "GenreId", "TrackId" },
                keyValues: new object[] { new Guid("88888888-8888-8888-8888-333333333333"), new Guid("99999999-9999-9999-9999-333333333333") });

            migrationBuilder.DeleteData(
                table: "TrackGenres",
                keyColumns: new[] { "GenreId", "TrackId" },
                keyValues: new object[] { new Guid("88888888-8888-8888-8888-444444444444"), new Guid("99999999-9999-9999-9999-444444444444") });

            migrationBuilder.DeleteData(
                table: "TrackGenres",
                keyColumns: new[] { "GenreId", "TrackId" },
                keyValues: new object[] { new Guid("88888888-8888-8888-8888-555555555555"), new Guid("99999999-9999-9999-9999-555555555555") });

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-111111111111"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-222222222222"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-333333333333"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-444444444444"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-555555555555"));

            migrationBuilder.DeleteData(
                table: "ListeningRooms",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-111111111111"));

            migrationBuilder.DeleteData(
                table: "ListeningRooms",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-222222222222"));

            migrationBuilder.DeleteData(
                table: "ListeningRooms",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-333333333333"));

            migrationBuilder.DeleteData(
                table: "ListeningRooms",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-444444444444"));

            migrationBuilder.DeleteData(
                table: "ListeningRooms",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-555555555555"));

            migrationBuilder.DeleteData(
                table: "Playlists",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-111111111111"));

            migrationBuilder.DeleteData(
                table: "Playlists",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-222222222222"));

            migrationBuilder.DeleteData(
                table: "Playlists",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-333333333333"));

            migrationBuilder.DeleteData(
                table: "Playlists",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-444444444444"));

            migrationBuilder.DeleteData(
                table: "Playlists",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-555555555555"));

            migrationBuilder.DeleteData(
                table: "Podcasts",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-111111111111"));

            migrationBuilder.DeleteData(
                table: "Podcasts",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-222222222222"));

            migrationBuilder.DeleteData(
                table: "Podcasts",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-333333333333"));

            migrationBuilder.DeleteData(
                table: "Podcasts",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-444444444444"));

            migrationBuilder.DeleteData(
                table: "Podcasts",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-555555555555"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-111111111111"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-222222222222"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-333333333333"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-444444444444"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-555555555555"));

            migrationBuilder.DeleteData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-111111111111"));

            migrationBuilder.DeleteData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-222222222222"));

            migrationBuilder.DeleteData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-333333333333"));

            migrationBuilder.DeleteData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-444444444444"));

            migrationBuilder.DeleteData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-555555555555"));

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-111111111111"));

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-222222222222"));

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-333333333333"));

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-444444444444"));

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-555555555555"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-111111111111"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-222222222222"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-444444444444"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-555555555555"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-222222222222"));

            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-111111111111"));

            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-333333333333"));

            migrationBuilder.AddForeignKey(
                name: "FK_Followers_Users_FollowedId",
                table: "Followers",
                column: "FollowedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
