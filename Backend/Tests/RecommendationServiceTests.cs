using Application.DTOs.Music;
using Domain.Entities.Music;
using Domain.Entities.Playlists;
using Infrastructure.Services;
using Sonara.Tests.Infrastructure;
using Xunit;

namespace Sonara.Tests;

public class RecommendationServiceTests : IDisposable
{
    private readonly SonaraTestDb _db = new();

    private readonly Guid _userId;
    private readonly Guid _artistAId;

    // Seeds: the user liked a Shoegaze track and played an Ambient one.
    private readonly Guid _likedTrackId;
    private readonly Guid _playedTrackId;

    // Candidates, none of them touched by the user.
    private readonly Guid _blendId;           // Shoegaze + Ambient
    private readonly Guid _artistAShoegazeId; // Shoegaze, by the artist the user liked
    private readonly Guid _shoegazeOnlyId;    // Shoegaze, by a stranger, more played
    private readonly Guid _ambientOnlyId;     // Ambient, by a stranger, far more played
    private readonly Guid _jazzTrackId;       // no genre overlap, most played in the catalog
    private readonly Guid _noGenreTrackId;    // no genres at all

    public RecommendationServiceTests()
    {
        using var context = _db.CreateContext();

        var user = TestData.NewUser("rec_user");
        var other = TestData.NewUser("rec_other");
        // An artist row is one-to-one with its user, so each needs its own owner.
        var ownerA = TestData.NewUser("rec_owner_a");
        var ownerB = TestData.NewUser("rec_owner_b");
        var ownerC = TestData.NewUser("rec_owner_c");
        context.Users.AddRange(user, other, ownerA, ownerB, ownerC);

        var artistA = TestData.NewArtist(ownerA.Id, "Nebula Drift");
        var artistB = TestData.NewArtist(ownerB.Id, "Slow Tide");
        var artistC = TestData.NewArtist(ownerC.Id, "Grid Runner");
        context.Artists.AddRange(artistA, artistB, artistC);

        var shoegaze = new Genre { Id = Guid.NewGuid(), Name = "Shoegaze" };
        var ambient = new Genre { Id = Guid.NewGuid(), Name = "Ambient" };
        var jazz = new Genre { Id = Guid.NewGuid(), Name = "Jazz" };
        context.Genres.AddRange(shoegaze, ambient, jazz);

        var liked = TestData.NewTrack(artistA.Id, "Quiet Orbit", playsCount: 10);
        var played = TestData.NewTrack(artistB.Id, "Tide Pool", playsCount: 20);
        var blend = TestData.NewTrack(artistC.Id, "Dust Bloom", playsCount: 5);
        var artistAShoegaze = TestData.NewTrack(artistA.Id, "Second Orbit", playsCount: 1);
        var shoegazeOnly = TestData.NewTrack(artistC.Id, "Grain", playsCount: 50);
        var ambientOnly = TestData.NewTrack(artistC.Id, "Halo", playsCount: 500);
        var jazzTrack = TestData.NewTrack(artistC.Id, "Blue Note", playsCount: 1000);
        var noGenreTrack = TestData.NewTrack(artistC.Id, "Untagged", playsCount: 900);
        context.Tracks.AddRange(liked, played, blend, artistAShoegaze, shoegazeOnly, ambientOnly, jazzTrack, noGenreTrack);

        context.TrackGenres.AddRange(
            new TrackGenre { TrackId = liked.Id, GenreId = shoegaze.Id },
            new TrackGenre { TrackId = played.Id, GenreId = ambient.Id },
            new TrackGenre { TrackId = blend.Id, GenreId = shoegaze.Id },
            new TrackGenre { TrackId = blend.Id, GenreId = ambient.Id },
            new TrackGenre { TrackId = artistAShoegaze.Id, GenreId = shoegaze.Id },
            new TrackGenre { TrackId = shoegazeOnly.Id, GenreId = shoegaze.Id },
            new TrackGenre { TrackId = ambientOnly.Id, GenreId = ambient.Id },
            new TrackGenre { TrackId = jazzTrack.Id, GenreId = jazz.Id });

        context.LikedTracks.Add(new LikedTrack { UserId = user.Id, TrackId = liked.Id, LikedAt = DateTime.UtcNow });
        context.ListeningHistories.Add(NewListen(user.Id, played.Id, minutesAgo: 5));

        // Another user listens to nothing but Jazz; it must not leak into the
        // caller's profile.
        for (var i = 0; i < 5; i++)
        {
            context.ListeningHistories.Add(NewListen(other.Id, jazzTrack.Id, minutesAgo: i));
        }

        context.SaveChanges();

        _userId = user.Id;
        _artistAId = artistA.Id;
        _likedTrackId = liked.Id;
        _playedTrackId = played.Id;
        _blendId = blend.Id;
        _artistAShoegazeId = artistAShoegaze.Id;
        _shoegazeOnlyId = shoegazeOnly.Id;
        _ambientOnlyId = ambientOnly.Id;
        _jazzTrackId = jazzTrack.Id;
        _noGenreTrackId = noGenreTrack.Id;
    }

    private static ListeningHistory NewListen(Guid userId, Guid trackId, int minutesAgo) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        TrackId = trackId,
        ListenedAt = DateTime.UtcNow.AddMinutes(-minutesAgo),
        DurationListenedMs = 60_000
    };

    private RecommendationService NewService() => new(_db.CreateContext());

    private static Guid[] Ids(RecommendationsDto result) => result.Items.Select(i => i.Track.Id).ToArray();

    [Fact]
    public async Task Ranks_candidates_by_genre_overlap_with_the_user_taste()
    {
        var result = await NewService().GetRecommendationsAsync(_userId, count: 4);

        Assert.Equal(RecommendationStrategy.Personalized, result.Strategy);

        // One like (weight 3) on Shoegaze beats one play (weight 1) on Ambient,
        // so the far more played Ambient-only track still lands last.
        Assert.Equal(
            new[] { _blendId, _artistAShoegazeId, _shoegazeOnlyId, _ambientOnlyId },
            Ids(result));

        Assert.Equal(new[] { 4.0, 3.0, 3.0, 1.0 }, result.Items.Select(i => i.Score).ToArray());
    }

    [Fact]
    public async Task Reports_the_genres_the_ranking_was_built_from()
    {
        var result = await NewService().GetRecommendationsAsync(_userId, count: 4);

        Assert.Equal(new[] { "Shoegaze", "Ambient" }, result.TopGenres);
        Assert.Equal(new[] { "Shoegaze", "Ambient" }, result.Items[0].MatchedGenres);
        Assert.Equal(new[] { "Ambient" }, result.Items[3].MatchedGenres);
    }

    [Fact]
    public async Task Prefers_a_familiar_artist_when_the_genre_pull_is_equal()
    {
        var result = await NewService().GetRecommendationsAsync(_userId, count: 4);

        var artistATrack = result.Items.Single(i => i.Track.Id == _artistAShoegazeId);
        var strangerTrack = result.Items.Single(i => i.Track.Id == _shoegazeOnlyId);

        // Same Shoegaze score, and the stranger's track is the more played one,
        // but the user already listens to artist A.
        Assert.Equal(artistATrack.Score, strangerTrack.Score);
        Assert.Equal(_artistAId, artistATrack.Track.ArtistId);
        Assert.True(
            result.Items.IndexOf(artistATrack) < result.Items.IndexOf(strangerTrack),
            "a track by an artist the user already plays should outrank an equally matching stranger");
    }

    [Fact]
    public async Task Repeated_plays_outweigh_a_single_like()
    {
        var listener = TestData.NewUser("rec_repeat_listener");
        await using (var context = _db.CreateContext())
        {
            context.Users.Add(listener);
            context.LikedTracks.Add(new LikedTrack { UserId = listener.Id, TrackId = _likedTrackId, LikedAt = DateTime.UtcNow });

            // Four Ambient plays (weight 4) against one Shoegaze like (weight 3).
            for (var i = 0; i < 4; i++)
            {
                context.ListeningHistories.Add(NewListen(listener.Id, _playedTrackId, minutesAgo: i));
            }

            await context.SaveChangesAsync();
        }

        var result = await NewService().GetRecommendationsAsync(listener.Id, count: 4);

        Assert.Equal(new[] { "Ambient", "Shoegaze" }, result.TopGenres);
        Assert.Equal(_blendId, result.Items[0].Track.Id);
        Assert.Equal(_ambientOnlyId, result.Items[1].Track.Id);
    }

    [Fact]
    public async Task Never_recommends_a_track_the_user_already_liked_or_played()
    {
        var result = await NewService().GetRecommendationsAsync(_userId, count: 50);

        Assert.DoesNotContain(_likedTrackId, Ids(result));
        Assert.DoesNotContain(_playedTrackId, Ids(result));
    }

    [Fact]
    public async Task Ignores_other_users_activity()
    {
        var result = await NewService().GetRecommendationsAsync(_userId, count: 4);

        // Another user played the Jazz track five times; the caller's profile and
        // ranking must be untouched by it.
        Assert.DoesNotContain("Jazz", result.TopGenres);
        Assert.DoesNotContain(_jazzTrackId, Ids(result));
    }

    [Fact]
    public async Task Tops_up_with_popular_tracks_when_the_genre_matches_run_out()
    {
        var result = await NewService().GetRecommendationsAsync(_userId, count: 10);

        Assert.Equal(RecommendationStrategy.Personalized, result.Strategy);

        // Four genre matches, then the most played tracks the user has not met.
        Assert.Equal(
            new[] { _blendId, _artistAShoegazeId, _shoegazeOnlyId, _ambientOnlyId, _jazzTrackId, _noGenreTrackId },
            Ids(result));

        Assert.All(result.Items.Skip(4), item =>
        {
            Assert.Equal(0, item.Score);
            Assert.Empty(item.MatchedGenres);
        });
    }

    [Fact]
    public async Task Falls_back_to_popular_tracks_for_a_user_with_no_activity()
    {
        var newcomer = TestData.NewUser("rec_newcomer");
        await using (var context = _db.CreateContext())
        {
            context.Users.Add(newcomer);
            await context.SaveChangesAsync();
        }

        var result = await NewService().GetRecommendationsAsync(newcomer.Id, count: 3);

        Assert.Equal(RecommendationStrategy.Popular, result.Strategy);
        Assert.Empty(result.TopGenres);
        Assert.Equal(new[] { _jazzTrackId, _noGenreTrackId, _ambientOnlyId }, Ids(result));
        Assert.All(result.Items, item => Assert.Equal(0, item.Score));
    }

    [Fact]
    public async Task Falls_back_to_popularity_when_the_only_activity_carries_no_genre()
    {
        var listener = TestData.NewUser("rec_untagged_listener");
        await using (var context = _db.CreateContext())
        {
            context.Users.Add(listener);
            context.ListeningHistories.Add(NewListen(listener.Id, _noGenreTrackId, minutesAgo: 1));
            await context.SaveChangesAsync();
        }

        var result = await NewService().GetRecommendationsAsync(listener.Id, count: 3);

        Assert.Equal(RecommendationStrategy.Popular, result.Strategy);
        // The untagged track was played, so it is still excluded.
        Assert.DoesNotContain(_noGenreTrackId, Ids(result));
        Assert.Equal(new[] { _jazzTrackId, _ambientOnlyId, _shoegazeOnlyId }, Ids(result));
    }

    [Fact]
    public async Task Projects_the_full_track_payload_the_player_needs()
    {
        var result = await NewService().GetRecommendationsAsync(_userId, count: 1);

        var track = Assert.Single(result.Items).Track;
        Assert.Equal("Dust Bloom", track.Title);
        Assert.Equal("Grid Runner", track.ArtistName);
        Assert.True(track.HasStream);
        Assert.False(track.IsLiked);
        Assert.Equal(new[] { "Ambient", "Shoegaze" }, track.Genres.OrderBy(g => g).ToArray());
    }

    [Fact]
    public async Task Clamps_the_requested_count()
    {
        Assert.Single((await NewService().GetRecommendationsAsync(_userId, count: 0)).Items);

        var all = await NewService().GetRecommendationsAsync(_userId, count: 5_000);
        Assert.True(all.Items.Count <= RecommendationService.MaxCount);
    }

    public void Dispose() => _db.Dispose();
}
