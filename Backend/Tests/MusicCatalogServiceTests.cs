using Application.Exceptions;
using Application.Interfaces.Services;
using Domain.Entities.Music;
using Domain.Entities.Playlists;
using Infrastructure.Services;
using Sonara.Tests.Infrastructure;
using Xunit;
using ValidationException = Application.Exceptions.ValidationException;

namespace Sonara.Tests;

public class MusicCatalogServiceTests : IDisposable
{
    private readonly SonaraTestDb _db = new();

    private Guid _userId;
    private Guid _otherUserId;
    private Guid _artistId;
    private Guid _albumId;
    private Guid _likedTrackId;
    private Guid _popularTrackId;
    private Guid _noMediaTrackId;

    public MusicCatalogServiceTests()
    {
        using var context = _db.CreateContext();

        var user = TestData.NewUser("catalog_user");
        var other = TestData.NewUser("catalog_other");
        var artistOwner = TestData.NewUser("catalog_artist_owner");
        context.Users.AddRange(user, other, artistOwner);

        var artist = TestData.NewArtist(artistOwner.Id, "Nebula Drift", verified: true);
        context.Artists.Add(artist);

        var album = TestData.NewAlbum(artist.Id, "Deep Field", "https://cdn.test/covers/deep-field.jpg");
        context.Albums.Add(album);

        var liked = TestData.NewTrack(artist.Id, "Quiet Orbit", album.Id, durationMs: 180_000, playsCount: 10,
            createdAt: new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        var popular = TestData.NewTrack(artist.Id, "Orbital Rush", album.Id, durationMs: 240_000, playsCount: 9_000,
            createdAt: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));
        var noMedia = TestData.NewTrack(artist.Id, "Missing Master", albumId: null, audioUrl: null,
            createdAt: new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        context.Tracks.AddRange(liked, popular, noMedia);

        var genre = new Genre { Id = Guid.NewGuid(), Name = "Shoegaze" };
        context.Genres.Add(genre);
        context.TrackGenres.Add(new TrackGenre { TrackId = liked.Id, GenreId = genre.Id });

        context.LikedTracks.Add(new LikedTrack { UserId = user.Id, TrackId = liked.Id, LikedAt = DateTime.UtcNow });
        context.LikedTracks.Add(new LikedTrack { UserId = other.Id, TrackId = liked.Id, LikedAt = DateTime.UtcNow });

        context.Playlists.Add(TestData.NewPlaylist(user.Id, "Nebula Public Mix"));
        context.Playlists.Add(TestData.NewPlaylist(user.Id, "Nebula Secret Mix", isPrivate: true));

        context.SaveChanges();

        _userId = user.Id;
        _otherUserId = other.Id;
        _artistId = artist.Id;
        _albumId = album.Id;
        _likedTrackId = liked.Id;
        _popularTrackId = popular.Id;
        _noMediaTrackId = noMedia.Id;
    }

    private MusicCatalogService NewService() => new(_db.CreateContext());

    [Fact]
    public async Task GetTracks_projects_artwork_genres_and_stream_availability()
    {
        var result = await NewService().GetTracksAsync(new TrackQuery(ArtistId: _artistId), _userId);

        var liked = result.Items.Single(t => t.Id == _likedTrackId);
        Assert.Equal("Quiet Orbit", liked.Title);
        Assert.Equal("Nebula Drift", liked.ArtistName);
        Assert.Equal("Deep Field", liked.AlbumTitle);
        Assert.Equal("https://cdn.test/covers/deep-field.jpg", liked.ArtworkUrl);
        Assert.Equal(180_000, liked.DurationMs);
        Assert.Equal(180.0, liked.DurationSeconds);
        Assert.Equal(new[] { "Shoegaze" }, liked.Genres);
        Assert.True(liked.HasStream);
        Assert.True(liked.IsLiked);
    }

    [Fact]
    public async Task GetTracks_falls_back_to_artist_avatar_when_track_has_no_album()
    {
        var result = await NewService().GetTracksAsync(new TrackQuery(ArtistId: _artistId), currentUserId: null);

        var noMedia = result.Items.Single(t => t.Id == _noMediaTrackId);
        Assert.Null(noMedia.AlbumId);
        Assert.Equal("https://cdn.test/artists/Nebula Drift.jpg", noMedia.ArtworkUrl);
        Assert.False(noMedia.HasStream);
    }

    [Fact]
    public async Task GetTracks_reports_isLiked_false_for_anonymous_requests()
    {
        var result = await NewService().GetTracksAsync(new TrackQuery(ArtistId: _artistId), currentUserId: null);

        Assert.All(result.Items, t => Assert.False(t.IsLiked));
    }

    [Fact]
    public async Task GetTracks_isLiked_is_per_user()
    {
        var stranger = TestData.NewUser("catalog_stranger");
        await using (var context = _db.CreateContext())
        {
            context.Users.Add(stranger);
            await context.SaveChangesAsync();
        }

        var result = await NewService().GetTracksAsync(new TrackQuery(ArtistId: _artistId), stranger.Id);

        Assert.False(result.Items.Single(t => t.Id == _likedTrackId).IsLiked);
    }

    [Fact]
    public async Task GetTracks_filters_by_genre_case_insensitively()
    {
        var result = await NewService().GetTracksAsync(new TrackQuery(Genre: "sHoEgAzE"), _userId);

        Assert.Equal(new[] { _likedTrackId }, result.Items.Select(t => t.Id).ToArray());
    }

    [Fact]
    public async Task GetTracks_sorts_by_popularity_when_requested()
    {
        var result = await NewService().GetTracksAsync(
            new TrackQuery(ArtistId: _artistId, Sort: TrackSortOrder.Popular),
            _userId);

        Assert.Equal(_popularTrackId, result.Items.First().Id);
    }

    [Fact]
    public async Task GetTracks_clamps_page_size_and_reports_paging_metadata()
    {
        var result = await NewService().GetTracksAsync(
            new TrackQuery(Page: 1, PageSize: 1, ArtistId: _artistId),
            _userId);

        Assert.Single(result.Items);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetTracks_normalises_out_of_range_paging_input()
    {
        var result = await NewService().GetTracksAsync(
            new TrackQuery(Page: 0, PageSize: 5_000, ArtistId: _artistId),
            _userId);

        Assert.Equal(1, result.PageIndex);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetTrackById_returns_full_details()
    {
        var details = await NewService().GetTrackByIdAsync(_likedTrackId, _userId);

        Assert.Equal(_likedTrackId, details.Id);
        Assert.Equal(_albumId, details.AlbumId);
        Assert.Equal("Deep Field", details.AlbumTitle);
        Assert.Equal("Album", details.AlbumType);
        Assert.True(details.ArtistVerified);
        Assert.Equal(2, details.LikesCount);
        Assert.True(details.IsLiked);
        Assert.True(details.HasStream);
    }

    [Fact]
    public async Task GetTrackById_reports_missing_media_without_failing()
    {
        var details = await NewService().GetTrackByIdAsync(_noMediaTrackId, _userId);

        Assert.False(details.HasStream);
        Assert.Equal(0, details.LikesCount);
    }

    [Fact]
    public async Task GetTrackById_throws_not_found_for_unknown_id()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => NewService().GetTrackByIdAsync(Guid.NewGuid(), _userId));
    }

    [Fact]
    public async Task GetTracksByIds_preserves_requested_order_and_skips_unknown_ids()
    {
        var requested = new[] { _popularTrackId, Guid.NewGuid(), _likedTrackId };

        var result = await NewService().GetTracksByIdsAsync(requested, _userId);

        Assert.Equal(new[] { _popularTrackId, _likedTrackId }, result.Select(t => t.Id).ToArray());
    }

    [Fact]
    public async Task GetTracksByIds_returns_empty_for_empty_input()
    {
        Assert.Empty(await NewService().GetTracksByIdsAsync(Array.Empty<Guid>(), _userId));
    }

    [Fact]
    public async Task GetTracksByIds_rejects_oversized_batches()
    {
        var ids = Enumerable.Range(0, MusicCatalogService.MaxBatchIds + 1).Select(_ => Guid.NewGuid()).ToArray();

        await Assert.ThrowsAsync<ValidationException>(() => NewService().GetTracksByIdsAsync(ids, _userId));
    }

    [Fact]
    public async Task GetAlbumById_returns_tracks_and_totals()
    {
        var album = await NewService().GetAlbumByIdAsync(_albumId, _userId);

        Assert.Equal("Deep Field", album.Title);
        Assert.Equal("Nebula Drift", album.ArtistName);
        Assert.Equal(2, album.TracksCount);
        Assert.Equal(420_000, album.TotalDurationMs);
        Assert.Equal(2, album.Tracks.Count);
    }

    [Fact]
    public async Task GetAlbumById_throws_not_found_for_unknown_id()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => NewService().GetAlbumByIdAsync(Guid.NewGuid(), _userId));
    }

    [Fact]
    public async Task GetArtistById_returns_albums_and_top_tracks_by_plays()
    {
        var artist = await NewService().GetArtistByIdAsync(_artistId, _userId);

        Assert.Equal("Nebula Drift", artist.Name);
        Assert.True(artist.Verified);
        Assert.Single(artist.Albums);
        Assert.Equal(_popularTrackId, artist.TopTracks.First().Id);
    }

    [Fact]
    public async Task GetArtistById_throws_not_found_for_unknown_id()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => NewService().GetArtistByIdAsync(Guid.NewGuid(), _userId));
    }

    [Theory]
    [InlineData("orbit")]
    [InlineData("ORBIT")]
    [InlineData("  Orbit  ")]
    public async Task Search_is_case_insensitive_and_trims_the_query(string query)
    {
        var result = await NewService().SearchAsync(query, limit: 10, _userId);

        Assert.Contains(result.Tracks.Items, t => t.Id == _likedTrackId);
        Assert.Equal("Orbit", result.Query, ignoreCase: true);
    }

    [Fact]
    public async Task Search_ranks_prefix_matches_first()
    {
        var result = await NewService().SearchAsync("orbit", limit: 10, _userId);

        Assert.Equal(_popularTrackId, result.Tracks.Items.First().Id);
    }

    [Fact]
    public async Task Search_covers_artists_albums_and_public_playlists()
    {
        var result = await NewService().SearchAsync("nebula", limit: 10, _userId);

        Assert.Contains(result.Artists.Items, a => a.Id == _artistId);
        Assert.Contains(result.Playlists.Items, p => p.Name == "Nebula Public Mix");
        Assert.DoesNotContain(result.Playlists.Items, p => p.Name == "Nebula Secret Mix");
    }

    [Fact]
    public async Task Search_hides_private_playlists_even_from_their_owner()
    {
        var result = await NewService().SearchAsync("secret", limit: 10, _userId);

        Assert.Empty(result.Playlists.Items);
        Assert.Equal(0, result.Playlists.Total);
    }

    [Fact]
    public async Task Search_reports_total_beyond_the_returned_page()
    {
        var result = await NewService().SearchAsync("orbit", limit: 1, _userId);

        Assert.Single(result.Tracks.Items);
        Assert.Equal(2, result.Tracks.Total);
        Assert.Equal(1, result.Limit);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a")]
    public async Task Search_returns_an_empty_result_for_blank_or_too_short_queries(string? query)
    {
        var result = await NewService().SearchAsync(query, limit: 10, _userId);

        Assert.Empty(result.Tracks.Items);
        Assert.Empty(result.Artists.Items);
        Assert.Empty(result.Albums.Items);
        Assert.Empty(result.Playlists.Items);
        Assert.Equal(0, result.TotalResults);
    }

    [Fact]
    public async Task Search_clamps_the_limit()
    {
        var result = await NewService().SearchAsync("orbit", limit: 10_000, _userId);

        Assert.Equal(MusicCatalogService.MaxSearchLimit, result.Limit);
    }

    [Fact]
    public async Task Search_returns_nothing_for_a_query_that_matches_no_row()
    {
        var result = await NewService().SearchAsync("zzzzzznotarealthing", limit: 10, _userId);

        Assert.Equal(0, result.TotalResults);
    }

    public void Dispose() => _db.Dispose();
}
