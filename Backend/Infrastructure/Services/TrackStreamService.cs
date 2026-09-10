namespace Infrastructure.Services;

using Application.DTOs.Music;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities.Music;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class TrackStreamService : ITrackStreamService
{
    private const int DefaultLifetimeMinutes = 60;
    private const int MinLifetimeMinutes = 5;
    private const int MaxLifetimeMinutes = 24 * 60;

    private readonly SonaraDbContext _context;
    private readonly IBlobService _blobService;
    private readonly ILogger<TrackStreamService> _logger;
    private readonly TimeSpan _lifetime;

    public TrackStreamService(
        SonaraDbContext context,
        IBlobService blobService,
        IConfiguration configuration,
        ILogger<TrackStreamService> logger)
    {
        _context = context;
        _blobService = blobService;
        _logger = logger;

        var minutes = int.TryParse(configuration["Media:StreamUrlLifetimeMinutes"], out var configured)
            ? configured
            : DefaultLifetimeMinutes;

        _lifetime = TimeSpan.FromMinutes(Math.Clamp(minutes, MinLifetimeMinutes, MaxLifetimeMinutes));
    }

    public async Task<TrackStreamDto> ResolveAsync(Guid id, CancellationToken ct = default)
    {
        var mediaItem = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new { t.Id, t.AudioUrl, DurationMs = (int)t.DurationMs })
            .FirstOrDefaultAsync(ct);

        if (mediaItem == null)
        {
            var episode = await _context.PodcastEpisodes
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new { e.Id, e.AudioUrl, DurationMs = (int)e.DurationMs })
                .FirstOrDefaultAsync(ct);

            if (episode != null)
            {
                mediaItem = new { episode.Id, episode.AudioUrl, episode.DurationMs };
            }
        }

        if (mediaItem == null)
        {
            throw new NotFoundException("MediaItem", id);
        }

        if (string.IsNullOrWhiteSpace(mediaItem.AudioUrl))
        {
            throw new MediaUnavailableException(id);
        }

        string? signedUrl;
        try
        {
            signedUrl = _blobService.TryCreateReadUrl(mediaItem.AudioUrl, _lifetime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build a stream URL for media item {MediaId}", id);
            throw new StorageUnavailableException(inner: ex);
        }

        var isSigned = !string.IsNullOrWhiteSpace(signedUrl);

        return new TrackStreamDto
        {
            TrackId = mediaItem.Id,
            Url = isSigned ? signedUrl! : mediaItem.AudioUrl,
            ContentType = ResolveContentType(mediaItem.AudioUrl),
            DurationMs = mediaItem.DurationMs,
            Mode = isSigned ? TrackStreamMode.SignedUrl : TrackStreamMode.DirectUrl,
            ExpiresAt = isSigned ? DateTime.UtcNow.Add(_lifetime).AddMinutes(-1) : null,
            SupportsRangeRequests = true
        };
    }

    internal static string ResolveContentType(string audioUrl)
    {
        var path = Uri.TryCreate(audioUrl, UriKind.Absolute, out var uri) ? uri.AbsolutePath : audioUrl;

        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".mp3" => "audio/mpeg",
            ".m4a" or ".mp4" or ".aac" => "audio/mp4",
            ".ogg" or ".oga" => "audio/ogg",
            ".opus" => "audio/opus",
            ".wav" => "audio/wav",
            ".flac" => "audio/flac",
            ".webm" => "audio/webm",
            _ => "application/octet-stream"
        };
    }
}
