namespace Infrastructure.Services;

using Application.DTOs.Music;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities.Music;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Розвʼязує джерело відтворення треку. Це єдиний компонент, який знає про фізичне
/// розташування медіа; назовні віддається лише готове посилання з обмеженим часом життя.
/// </summary>
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

    public async Task<TrackStreamDto> ResolveAsync(Guid trackId, CancellationToken ct = default)
    {
        var track = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id == trackId)
            .Select(t => new { t.Id, t.AudioUrl, t.DurationMs })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Track), trackId);

        if (string.IsNullOrWhiteSpace(track.AudioUrl))
        {
            throw new MediaUnavailableException(trackId);
        }

        string? signedUrl;
        try
        {
            signedUrl = _blobService.TryCreateReadUrl(track.AudioUrl, _lifetime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build a stream URL for track {TrackId}", trackId);
            throw new StorageUnavailableException(inner: ex);
        }

        var isSigned = !string.IsNullOrWhiteSpace(signedUrl);

        return new TrackStreamDto
        {
            TrackId = track.Id,
            Url = isSigned ? signedUrl! : track.AudioUrl,
            ContentType = ResolveContentType(track.AudioUrl),
            DurationMs = track.DurationMs,
            Mode = isSigned ? TrackStreamMode.SignedUrl : TrackStreamMode.DirectUrl,
            // Час життя навмисно трохи занижений відносно реального SAS, щоб плеєр
            // встиг перезапитати посилання до того, як воно протухне під час відтворення.
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
