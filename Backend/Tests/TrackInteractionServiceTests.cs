using Application.DTOs.Music;
using Application.Exceptions;
using Domain.Entities.Music;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Sonara.Tests.Infrastructure;
using Xunit;

namespace Sonara.Tests;

public class TrackInteractionServiceTests : IDisposable
{
    private readonly SonaraTestDb _db = new();

    private readonly Guid _userId;
    private readonly Guid _otherUserId;
    private readonly Guid _trackId;
    private readonly Guid _shortTrackId;

    public TrackInteractionServiceTests()
    {
        using var context = _db.CreateContext();

        var user = TestData.NewUser("interaction_user");
        var other = TestData.NewUser("interaction_other");
        var owner = TestData.NewUser("interaction_artist_owner");
        context.Users.AddRange(user, other, owner);

        var artist = TestData.NewArtist(owner.Id, "Low Pass");
        context.Artists.Add(artist);

        var track = TestData.NewTrack(artist.Id, "Cutoff", durationMs: 200_000);
        var shortTrack = TestData.NewTrack(artist.Id, "Blip", durationMs: 12_000);
        context.Tracks.AddRange(track, shortTrack);

        context.SaveChanges();

        _userId = user.Id;
        _otherUserId = other.Id;
        _trackId = track.Id;
        _shortTrackId = shortTrack.Id;
    }

    private TrackInteractionService NewService() => new(_db.CreateContext());

    [Fact]
    public async Task Like_returns_the_resulting_state()
    {
        var state = await NewService().LikeAsync(_trackId, _userId);

        Assert.Equal(_trackId, state.TrackId);
        Assert.True(state.IsLiked);
        Assert.Equal(1, state.LikesCount);
        Assert.NotNull(state.LikedAt);
    }

    [Fact]
    public async Task Like_is_idempotent()
    {
        await NewService().LikeAsync(_trackId, _userId);
        var state = await NewService().LikeAsync(_trackId, _userId);

        Assert.True(state.IsLiked);
        Assert.Equal(1, state.LikesCount);

        await using var context = _db.CreateContext();
        Assert.Equal(1, await context.LikedTracks.CountAsync(l => l.TrackId == _trackId && l.UserId == _userId));
    }

    [Fact]
    public async Task Like_counts_every_user_but_reports_only_the_caller_state()
    {
        await NewService().LikeAsync(_trackId, _otherUserId);
        var state = await NewService().GetLikeStateAsync(_trackId, _userId);

        Assert.False(state.IsLiked);
        Assert.Equal(1, state.LikesCount);
        Assert.Null(state.LikedAt);
    }

    [Fact]
    public async Task Unlike_is_idempotent()
    {
        await NewService().LikeAsync(_trackId, _userId);

        var first = await NewService().UnlikeAsync(_trackId, _userId);
        var second = await NewService().UnlikeAsync(_trackId, _userId);

        Assert.False(first.IsLiked);
        Assert.False(second.IsLiked);
        Assert.Equal(0, second.LikesCount);
    }

    [Fact]
    public async Task Like_unlike_and_state_reject_unknown_tracks()
    {
        var unknown = Guid.NewGuid();

        await Assert.ThrowsAsync<NotFoundException>(() => NewService().LikeAsync(unknown, _userId));
        await Assert.ThrowsAsync<NotFoundException>(() => NewService().UnlikeAsync(unknown, _userId));
        await Assert.ThrowsAsync<NotFoundException>(() => NewService().GetLikeStateAsync(unknown, _userId));
    }

    [Fact]
    public async Task GetLikedTracks_returns_only_the_callers_likes_newest_first()
    {
        await NewService().LikeAsync(_trackId, _userId);
        await Task.Delay(10);
        await NewService().LikeAsync(_shortTrackId, _userId);
        await NewService().LikeAsync(_trackId, _otherUserId);

        var page = await NewService().GetLikedTracksAsync(_userId, page: 1, pageSize: 10);

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(_shortTrackId, page.Items.First().Id);
        Assert.All(page.Items, t => Assert.True(t.IsLiked));
    }

    [Fact]
    public async Task RegisterListen_rejects_a_play_that_is_too_short()
    {
        var result = await NewService().RegisterListenAsync(_trackId, _userId, durationListenedMs: 5_000);

        Assert.Equal(ListenRecordStatus.TooShort, result.Status);
        Assert.False(result.Recorded);
        Assert.Null(result.ListenedAt);
        Assert.Equal(TrackInteractionService.MeaningfulListenMs, result.RequiredListenedMs);

        await using var context = _db.CreateContext();
        Assert.Equal(0, await context.ListeningHistories.CountAsync(h => h.TrackId == _trackId));
        Assert.Equal(0, (await context.Tracks.SingleAsync(t => t.Id == _trackId)).PlaysCount);
    }

    [Fact]
    public async Task RegisterListen_treats_a_missing_duration_as_too_short()
    {
        var result = await NewService().RegisterListenAsync(_trackId, _userId, durationListenedMs: null);

        Assert.Equal(ListenRecordStatus.TooShort, result.Status);
    }

    [Fact]
    public async Task RegisterListen_records_a_meaningful_play_and_increments_the_counter()
    {
        var result = await NewService().RegisterListenAsync(_trackId, _userId, durationListenedMs: 45_000);

        Assert.Equal(ListenRecordStatus.Recorded, result.Status);
        Assert.True(result.Recorded);
        Assert.Equal(1, result.PlaysCount);
        Assert.NotNull(result.ListenedAt);

        await using var context = _db.CreateContext();
        var entry = await context.ListeningHistories.SingleAsync(h => h.TrackId == _trackId && h.UserId == _userId);
        Assert.Equal(45_000, entry.DurationListenedMs);
        Assert.Equal(1, (await context.Tracks.SingleAsync(t => t.Id == _trackId)).PlaysCount);
    }

    [Fact]
    public async Task RegisterListen_uses_half_the_track_for_tracks_shorter_than_the_threshold()
    {
        var result = await NewService().RegisterListenAsync(_shortTrackId, _userId, durationListenedMs: 6_000);

        Assert.Equal(ListenRecordStatus.Recorded, result.Status);
        Assert.Equal(6_000, result.RequiredListenedMs);
    }

    [Fact]
    public async Task RegisterListen_throttles_repeated_calls_so_progress_ticks_cannot_flood_history()
    {
        await NewService().RegisterListenAsync(_trackId, _userId, 60_000);

        var second = await NewService().RegisterListenAsync(_trackId, _userId, 60_000);
        var third = await NewService().RegisterListenAsync(_trackId, _userId, 60_000);

        Assert.Equal(ListenRecordStatus.Throttled, second.Status);
        Assert.Equal(ListenRecordStatus.Throttled, third.Status);

        await using var context = _db.CreateContext();
        Assert.Equal(1, await context.ListeningHistories.CountAsync(h => h.TrackId == _trackId && h.UserId == _userId));
        Assert.Equal(1, (await context.Tracks.SingleAsync(t => t.Id == _trackId)).PlaysCount);
    }

    [Fact]
    public async Task RegisterListen_does_not_throttle_a_different_user()
    {
        await NewService().RegisterListenAsync(_trackId, _userId, 60_000);
        var other = await NewService().RegisterListenAsync(_trackId, _otherUserId, 60_000);

        Assert.Equal(ListenRecordStatus.Recorded, other.Status);
        Assert.Equal(2, other.PlaysCount);
    }

    [Fact]
    public async Task RegisterListen_rejects_an_unknown_track()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => NewService().RegisterListenAsync(Guid.NewGuid(), _userId, 60_000));
    }

    [Fact]
    public async Task GetHistory_returns_the_callers_plays_with_full_track_data()
    {
        await NewService().RegisterListenAsync(_trackId, _userId, 60_000);
        await NewService().RegisterListenAsync(_trackId, _otherUserId, 60_000);

        var page = await NewService().GetHistoryAsync(_userId, page: 1, pageSize: 10);

        var entry = Assert.Single(page.Items);
        Assert.Equal(_trackId, entry.Track.Id);
        Assert.Equal("Cutoff", entry.Track.Title);
        Assert.Equal("Low Pass", entry.Track.ArtistName);
        Assert.Equal(60_000, entry.DurationListenedMs);
    }

    [Fact]
    public async Task GetHistory_is_paginated()
    {
        await using (var context = _db.CreateContext())
        {
            for (var i = 0; i < 5; i++)
            {
                context.ListeningHistories.Add(new ListeningHistory
                {
                    UserId = _userId,
                    TrackId = _trackId,
                    ListenedAt = DateTime.UtcNow.AddMinutes(-i),
                    DurationListenedMs = 60_000
                });
            }

            await context.SaveChangesAsync();
        }

        var page = await NewService().GetHistoryAsync(_userId, page: 2, pageSize: 2);

        Assert.Equal(5, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(2, page.PageIndex);
        Assert.True(page.HasPreviousPage);
    }

    public void Dispose() => _db.Dispose();
}
