using Application.DTOs.Music;
using Application.Exceptions;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Sonara.Tests.Infrastructure;
using Xunit;

namespace Sonara.Tests;

public class TrackStreamServiceTests : IDisposable
{
    private readonly SonaraTestDb _db = new();
    private readonly FakeBlobService _blob = new();

    private readonly Guid _trackId;
    private readonly Guid _trackWithoutMediaId;

    public TrackStreamServiceTests()
    {
        using var context = _db.CreateContext();

        var owner = TestData.NewUser("stream_owner");
        context.Users.Add(owner);

        var artist = TestData.NewArtist(owner.Id, "Signal Chain");
        context.Artists.Add(artist);

        var playable = TestData.NewTrack(artist.Id, "Carrier Wave", durationMs: 195_000,
            audioUrl: "https://storage.test/audio/tracks/carrier.mp3");
        var silent = TestData.NewTrack(artist.Id, "Lost Master", audioUrl: null);
        context.Tracks.AddRange(playable, silent);

        context.SaveChanges();

        _trackId = playable.Id;
        _trackWithoutMediaId = silent.Id;
    }

    private TrackStreamService NewService(params (string Key, string Value)[] config) =>
        new(_db.CreateContext(), _blob, TestConfiguration.Create(config), NullLogger<TrackStreamService>.Instance);

    [Fact]
    public async Task Resolve_returns_a_signed_url_when_storage_can_sign()
    {
        var result = await NewService().ResolveAsync(_trackId);

        Assert.Equal(_trackId, result.TrackId);
        Assert.Equal(TrackStreamMode.SignedUrl, result.Mode);
        Assert.StartsWith("https://storage.test/audio/tracks/carrier.mp3?sig=", result.Url);
        Assert.Equal("audio/mpeg", result.ContentType);
        Assert.Equal(195_000, result.DurationMs);
        Assert.True(result.SupportsRangeRequests);
        Assert.NotNull(result.ExpiresAt);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Resolve_honours_the_configured_lifetime()
    {
        var result = await NewService(("Media:StreamUrlLifetimeMinutes", "15")).ResolveAsync(_trackId);

        Assert.Contains("ttl=15", result.Url);
        Assert.True(result.ExpiresAt < DateTime.UtcNow.AddMinutes(15));
    }

    [Fact]
    public async Task Resolve_clamps_an_absurd_configured_lifetime()
    {
        var result = await NewService(("Media:StreamUrlLifetimeMinutes", "999999")).ResolveAsync(_trackId);

        Assert.True(result.ExpiresAt <= DateTime.UtcNow.AddDays(1));
    }

    [Fact]
    public async Task Resolve_falls_back_to_the_stored_url_when_signing_is_unavailable()
    {
        _blob.ReadUrlFactory = (_, _) => null;

        var result = await NewService().ResolveAsync(_trackId);

        Assert.Equal(TrackStreamMode.DirectUrl, result.Mode);
        Assert.Equal("https://storage.test/audio/tracks/carrier.mp3", result.Url);
        Assert.Null(result.ExpiresAt);
    }

    [Fact]
    public async Task Resolve_throws_not_found_for_an_unknown_track()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => NewService().ResolveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Resolve_distinguishes_missing_media_from_a_missing_track()
    {
        var ex = await Assert.ThrowsAsync<MediaUnavailableException>(
            () => NewService().ResolveAsync(_trackWithoutMediaId));

        Assert.Equal(_trackWithoutMediaId, ex.TrackId);
    }

    [Fact]
    public async Task Resolve_wraps_storage_failures_without_leaking_them()
    {
        _blob.ReadUrlFactory = (_, _) => throw new InvalidOperationException("azure exploded: account key rejected");

        var ex = await Assert.ThrowsAsync<StorageUnavailableException>(() => NewService().ResolveAsync(_trackId));

        Assert.DoesNotContain("azure exploded", ex.Message);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Theory]
    [InlineData("https://cdn.test/a/b.mp3", "audio/mpeg")]
    [InlineData("https://cdn.test/a/b.m4a", "audio/mp4")]
    [InlineData("https://cdn.test/a/b.OGG", "audio/ogg")]
    [InlineData("https://cdn.test/a/b.flac", "audio/flac")]
    [InlineData("https://cdn.test/a/b.wav?sig=x", "audio/wav")]
    [InlineData("https://cdn.test/a/b.xyz", "application/octet-stream")]
    [InlineData("https://cdn.test/a/b", "application/octet-stream")]
    public void ResolveContentType_maps_known_audio_extensions(string url, string expected)
    {
        Assert.Equal(expected, TrackStreamService.ResolveContentType(url));
    }

    public void Dispose() => _db.Dispose();
}
