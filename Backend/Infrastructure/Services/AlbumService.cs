using Application.DTOs.Music;
using Application.Enums;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities.Music;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NBitcoin.Secp256k1;

namespace Infrastructure.Services;
public class AlbumService : IAlbumService
{
    private readonly SonaraDbContext _db;
    private readonly IBlobService _blobService;

    public AlbumService(SonaraDbContext db, IBlobService blobService)
    {
        _db = db;
        _blobService = blobService;
    }

    public async Task<IEnumerable<AlbumDto>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Albums
            .AsNoTracking()
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .Select(a => new AlbumDto
            {
                Id = a.Id,
                Title = a.Title,
                CoverUrl = a.CoverUrl,
                Type = a.Type,
                ReleaseDate = a.ReleaseDate,
                ArtistId = a.ArtistId,
                ArtistName = a.Artist.Name,
                TracksCount = a.Tracks.Count,
                TotalDurationMs = a.Tracks.Sum(t => t.DurationMs),
                Tracks = a.Tracks.Select(t => new TrackDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    ArtistId = t.ArtistId,
                    ArtistName = a.Artist.Name,
                    AlbumId = t.Id,
                    AlbumTitle = a.Title,
                    ArtworkUrl = a.CoverUrl,
                    DurationMs = t.DurationMs,
                    PlaysCount = t.PlaysCount,
                    CreatedAt = t.CreatedAt
                }).ToList()
            })
            .ToListAsync(ct);
    }

    public async Task<AlbumSummaryDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Albums
            .AsNoTracking()
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .Where(a => a.Id == id)
            .Select(a => new AlbumSummaryDto
            {
                Id = a.Id,
                Title = a.Title,
                CoverUrl = a.CoverUrl,
                Type = a.Type,
                ReleaseDate = a.ReleaseDate,
                ArtistId = a.ArtistId,
                ArtistName = a.Artist.Name,
                TracksCount = a.Tracks.Count
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AlbumDto> CreateAsync(CreateAlbumDto dto, Guid currentUserId, CancellationToken ct)
    {
        string coverUrl = string.Empty;
        if (dto.CoverImage != null)
        {
            coverUrl = await _blobService.UploadFileAsync(dto.CoverImage, BlobFolder.AlbumsCovers);
        }

        var artist = await _db.Artists
            .FirstOrDefaultAsync(a => a.UserId == currentUserId, ct);

        if (artist == null)
        {
            artist = new Artist
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
            };
            _db.Artists.Add(artist);
            await _db.SaveChangesAsync(ct);
        }

        var album = new Album
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            ArtistId = artist.Id,
            CoverUrl = coverUrl,
            ReleaseDate = dto.ReleaseDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.Albums.Add(album);
        await _db.SaveChangesAsync(ct);

        return new AlbumDto
        {
            Id = album.Id,
            Title = album.Title,
            CoverUrl = album.CoverUrl,
            Type = album.Type,
            ReleaseDate = album.ReleaseDate,
            ArtistId = album.ArtistId,
            ArtistName = artist.Name,
            TracksCount = 0,
            TotalDurationMs = 0,
            Tracks = new List<TrackDto>()
        };
    }

    public async Task<AlbumDto?> UpdateAsync(Guid id, UpdateAlbumDto dto, Guid currentUserId, CancellationToken ct)
    {
        var album = await _db.Albums
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (album == null) return null;

        if (album.ArtistId != currentUserId)
            throw new UnauthorizedAccessException("You are not allowed to update this album.");

        if (dto.Title != null) album.Title = dto.Title;
        if (dto.ReleaseDate.HasValue) album.ReleaseDate = dto.ReleaseDate.Value;

        if (dto.CoverImage != null)
        {
            album.CoverUrl = await _blobService.ReplaceFileAsync(dto.CoverImage, album.CoverUrl, BlobFolder.AlbumsCovers);
        }

        await _db.SaveChangesAsync(ct);

        return new AlbumDto
        {
            Id = album.Id,
            Title = album.Title,
            CoverUrl = album.CoverUrl,
            Type = album.Type,
            ReleaseDate = album.ReleaseDate,
            ArtistId = album.ArtistId,
            ArtistName = album.Artist?.Name ?? string.Empty,
            TracksCount = album.Tracks.Count,
            TotalDurationMs = album.Tracks.Sum(t => t.DurationMs),
            Tracks = album.Tracks.Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                ArtistId = t.ArtistId,
                ArtistName = album.Artist?.Name ?? string.Empty,
                AlbumId = album.Id,
                AlbumTitle = album.Title,
                ArtworkUrl = album.CoverUrl,
                DurationMs = t.DurationMs,
                PlaysCount = t.PlaysCount,
                CreatedAt = t.CreatedAt
            }).ToList()
        };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct)
    {
        var album = await _db.Albums.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (album == null) return false;

        if (album.ArtistId != currentUserId)
            throw new UnauthorizedAccessException("You are not allowed to delete this album.");

        if (!string.IsNullOrWhiteSpace(album.CoverUrl))
        {
            await _blobService.DeleteFileAsync(album.CoverUrl, BlobFolder.AlbumsCovers);
        }

        _db.Albums.Remove(album);
        await _db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<AlbumDto?> AddTrackToAlbumAsync(Guid albumId, string trackName, Guid currentUserId, CancellationToken ct)
    {
        var album = await _db.Albums
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.Id == albumId, ct);

        if (album == null) return null;

        if (album.Artist.UserId != currentUserId)
            throw new UnauthorizedAccessException("You are not allowed to modify this album.");

        var track = await _db.Tracks
            .Include(t => t.Artist)
            .FirstOrDefaultAsync(t => t.Title == trackName, ct);

        if (track == null) return null;

        // Прив'язуємо трек до альбому
        track.AlbumId = album.Id;

        await _db.SaveChangesAsync(ct);

        // Повторно збираємо актуальний стан альбому для відповіді
        return new AlbumDto
        {
            Id = album.Id,
            Title = album.Title,
            CoverUrl = album.CoverUrl,
            Type = album.Type,
            ReleaseDate = album.ReleaseDate,
            ArtistId = album.ArtistId,
            ArtistName = album.Artist?.Name ?? string.Empty,
            TracksCount = album.Tracks.Count,
            TotalDurationMs = album.Tracks.Sum(t => t.DurationMs),
            Tracks = album.Tracks.Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                ArtistId = t.ArtistId,
                ArtistName = t.Artist?.Name ?? album.Artist?.Name ?? string.Empty,
                AlbumId = album.Id,
                AlbumTitle = album.Title,
                ArtworkUrl = album.CoverUrl,
                DurationMs = t.DurationMs,
                PlaysCount = t.PlaysCount,
                CreatedAt = t.CreatedAt
            }).ToList()
        };
    }
}