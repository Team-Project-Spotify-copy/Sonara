using Application.DTOs.Playlists;
using Application.Exceptions;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Sonara.Tests.Infrastructure;
using Xunit;
using ValidationException = Application.Exceptions.ValidationException;

namespace Sonara.Tests;

public class PlaylistServiceTests : IDisposable
{
    private readonly SonaraTestDb _db = new();
    private readonly FakeBlobService _blob = new();

    private readonly Guid _ownerId;
    private readonly Guid _strangerId;
    private readonly Guid _publicPlaylistId;
    private readonly Guid _privatePlaylistId;
    private readonly Guid _trackId;
    private readonly Guid _otherTrackId;

    public PlaylistServiceTests()
    {
        using var context = _db.CreateContext();

        var owner = TestData.NewUser("playlist_owner");
        var stranger = TestData.NewUser("playlist_stranger");
        var artistOwner = TestData.NewUser("playlist_artist_owner");
        context.Users.AddRange(owner, stranger, artistOwner);

        var artist = TestData.NewArtist(artistOwner.Id, "Slow Motion");
        context.Artists.Add(artist);

        var track = TestData.NewTrack(artist.Id, "Drift", durationMs: 150_000);
        var otherTrack = TestData.NewTrack(artist.Id, "Glide", durationMs: 90_000);
        context.Tracks.AddRange(track, otherTrack);

        var publicPlaylist = TestData.NewPlaylist(owner.Id, "Evening");
        var privatePlaylist = TestData.NewPlaylist(owner.Id, "Hidden", isPrivate: true);
        context.Playlists.AddRange(publicPlaylist, privatePlaylist);

        context.SaveChanges();

        _ownerId = owner.Id;
        _strangerId = stranger.Id;
        _publicPlaylistId = publicPlaylist.Id;
        _privatePlaylistId = privatePlaylist.Id;
        _trackId = track.Id;
        _otherTrackId = otherTrack.Id;
    }

    private PlaylistService NewService() => new(_db.CreateContext(), _blob);

    [Fact]
    public async Task Create_works_without_a_cover_image()
    {
        var created = await NewService().CreateAsync(_ownerId, new CreatePlaylistRequest("Focus", null, false, null));

        Assert.Equal("Focus", created.Name);
        Assert.Null(created.CoverUrl);
        Assert.Equal(0, created.TracksCount);
        Assert.True(created.IsOwner);
        Assert.Equal("playlist_owner", created.OwnerUsername);
        Assert.Empty(_blob.Uploaded);
    }

    [Fact]
    public async Task Create_trims_the_name_and_normalises_a_blank_description()
    {
        var created = await NewService().CreateAsync(_ownerId, new CreatePlaylistRequest("  Focus  ", "   ", false, null));

        Assert.Equal("Focus", created.Name);
        Assert.Null(created.Description);
    }

    [Fact]
    public async Task Create_rejects_a_blank_name()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => NewService().CreateAsync(_ownerId, new CreatePlaylistRequest("   ", null, false, null)));
    }

    [Fact]
    public async Task Create_rejects_an_overlong_name()
    {
        var name = new string('x', PlaylistService.MaxNameLength + 1);

        await Assert.ThrowsAsync<ValidationException>(
            () => NewService().CreateAsync(_ownerId, new CreatePlaylistRequest(name, null, false, null)));
    }

    [Fact]
    public async Task GetById_lets_anyone_read_a_public_playlist()
    {
        var dto = await NewService().GetByIdAsync(_publicPlaylistId, requestingUserId: null);

        Assert.Equal("Evening", dto.Name);
        Assert.False(dto.IsOwner);
    }

    [Fact]
    public async Task GetById_hides_a_private_playlist_from_other_users()
    {
        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => NewService().GetByIdAsync(_privatePlaylistId, _strangerId));

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => NewService().GetByIdAsync(_privatePlaylistId, requestingUserId: null));
    }

    [Fact]
    public async Task GetById_lets_the_owner_read_their_private_playlist()
    {
        var dto = await NewService().GetByIdAsync(_privatePlaylistId, _ownerId);

        Assert.True(dto.IsPrivate);
        Assert.True(dto.IsOwner);
    }

    [Fact]
    public async Task GetById_throws_not_found_for_an_unknown_playlist()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => NewService().GetByIdAsync(Guid.NewGuid(), _ownerId));
    }

    [Fact]
    public async Task GetMyPlaylists_returns_only_the_callers_playlists()
    {
        var mine = await NewService().GetMyPlaylistsAsync(_ownerId);
        var theirs = await NewService().GetMyPlaylistsAsync(_strangerId);

        Assert.Equal(2, mine.Count);
        Assert.All(mine, p => Assert.True(p.IsOwner));
        Assert.Empty(theirs);
    }

    [Fact]
    public async Task Mutating_another_users_playlist_is_forbidden()
    {
        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => NewService().UpdateAsync(_publicPlaylistId, _strangerId, new UpdatePlaylistRequest("Hijacked", null, false, null)));

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => NewService().DeleteAsync(_publicPlaylistId, _strangerId));

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => NewService().AddTrackAsync(_publicPlaylistId, _strangerId, _trackId));

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => NewService().RemoveTrackAsync(_publicPlaylistId, _strangerId, _trackId));
    }

    [Fact]
    public async Task A_forbidden_update_leaves_the_playlist_untouched()
    {
        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => NewService().UpdateAsync(_publicPlaylistId, _strangerId, new UpdatePlaylistRequest("Hijacked", null, true, null)));

        await using var context = _db.CreateContext();
        var playlist = await context.Playlists.SingleAsync(p => p.Id == _publicPlaylistId);
        Assert.Equal("Evening", playlist.Name);
        Assert.False(playlist.IsPrivate);
    }

    [Fact]
    public async Task AddTrack_returns_the_updated_playlist()
    {
        var dto = await NewService().AddTrackAsync(_publicPlaylistId, _ownerId, _trackId);

        Assert.Equal(1, dto.TracksCount);
        Assert.Equal(150_000, dto.TotalDurationMs);
    }

    [Fact]
    public async Task AddTrack_is_idempotent_and_never_duplicates_the_link()
    {
        await NewService().AddTrackAsync(_publicPlaylistId, _ownerId, _trackId);
        var dto = await NewService().AddTrackAsync(_publicPlaylistId, _ownerId, _trackId);

        Assert.Equal(1, dto.TracksCount);

        await using var context = _db.CreateContext();
        Assert.Equal(1, await context.PlaylistTracks.CountAsync(pt => pt.PlaylistId == _publicPlaylistId));
    }

    [Fact]
    public async Task AddTrack_rejects_an_unknown_track()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => NewService().AddTrackAsync(_publicPlaylistId, _ownerId, Guid.NewGuid()));
    }

    [Fact]
    public async Task AddTrack_rejects_an_unknown_playlist()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => NewService().AddTrackAsync(Guid.NewGuid(), _ownerId, _trackId));
    }

    [Fact]
    public async Task RemoveTrack_is_idempotent()
    {
        await NewService().AddTrackAsync(_publicPlaylistId, _ownerId, _trackId);

        var first = await NewService().RemoveTrackAsync(_publicPlaylistId, _ownerId, _trackId);
        var second = await NewService().RemoveTrackAsync(_publicPlaylistId, _ownerId, _trackId);

        Assert.Equal(0, first.TracksCount);
        Assert.Equal(0, second.TracksCount);
    }

    [Fact]
    public async Task GetTracks_returns_positions_in_the_order_tracks_were_added()
    {
        await NewService().AddTrackAsync(_publicPlaylistId, _ownerId, _trackId);
        await Task.Delay(10);
        await NewService().AddTrackAsync(_publicPlaylistId, _ownerId, _otherTrackId);

        var rows = await NewService().GetTracksAsync(_publicPlaylistId, _ownerId);

        Assert.Equal(new[] { 0, 1 }, rows.Select(r => r.Position).ToArray());
        Assert.Equal(new[] { _trackId, _otherTrackId }, rows.Select(r => r.Track.Id).ToArray());
        Assert.Equal("Slow Motion", rows[0].Track.ArtistName);
        Assert.True(rows[0].Track.HasStream);
    }

    [Fact]
    public async Task GetTracks_respects_playlist_visibility()
    {
        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => NewService().GetTracksAsync(_privatePlaylistId, _strangerId));

        Assert.Empty(await NewService().GetTracksAsync(_privatePlaylistId, _ownerId));
    }

    [Fact]
    public async Task Delete_removes_the_playlist_and_its_cover()
    {
        var created = await NewService().CreateAsync(_ownerId, new CreatePlaylistRequest("Temp", null, false, null));

        await using (var seed = _db.CreateContext())
        {
            var playlist = await seed.Playlists.SingleAsync(p => p.Id == created.Id);
            playlist.CoverUrl = "https://cdn.test/covers/temp.jpg";
            await seed.SaveChangesAsync();
        }

        await NewService().DeleteAsync(created.Id, _ownerId);

        await using var context = _db.CreateContext();
        Assert.False(await context.Playlists.AnyAsync(p => p.Id == created.Id));
        Assert.Contains("https://cdn.test/covers/temp.jpg", _blob.Deleted);
    }

    [Fact]
    public async Task Deleting_a_playlist_removes_its_track_links_but_not_the_tracks()
    {
        await NewService().AddTrackAsync(_publicPlaylistId, _ownerId, _trackId);
        await NewService().DeleteAsync(_publicPlaylistId, _ownerId);

        await using var context = _db.CreateContext();
        Assert.False(await context.PlaylistTracks.AnyAsync(pt => pt.PlaylistId == _publicPlaylistId));
        Assert.True(await context.Tracks.AnyAsync(t => t.Id == _trackId));
    }

    public void Dispose() => _db.Dispose();
}
