namespace Infrastructure.Services;

using Application.DTOs.Music;
using Application.Enums;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities.Music;

public class AdminMusicService : IAdminMusicService
{
    private readonly SonaraDbContext _context;
    private readonly IBlobService _blobService;

    public AdminMusicService(SonaraDbContext context, IBlobService blobService)
    {
        _context = context;
        _blobService = blobService;
    }

    public async Task<Guid> CreateTrackAsync(CreateTrackDto dto)
    {
        if (dto.AudioFile == null || dto.AudioFile.Length == 0)
        {
            throw new ArgumentException("File not found");
        }

        string audioUrl = await _blobService.UploadFileAsync(dto.AudioFile, BlobFolder.MusicTracks);

        var track = new Track
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            DurationMs = (int)Math.Round(dto.DurationSeconds * 1000),
            AudioUrl = audioUrl,
            ArtistId = dto.ArtistId,
            AlbumId = dto.AlbumId,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.GenreIds != null && dto.GenreIds.Any())
        {
            track.TrackGenres = dto.GenreIds.Select(genreId => new TrackGenre
            {
                TrackId = track.Id,
                GenreId = genreId
            }).ToList();
        }

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        return track.Id;
    }

    public async Task<Guid> CreateAlbumAsync(CreateAlbumDto dto)
    {
        if (dto.CoverImage == null || dto.CoverImage.Length == 0)
        {
            throw new ArgumentException("File not found");
        }

        string imageUrl = await _blobService.UploadFileAsync(dto.CoverImage, BlobFolder.AlbumsCovers);

        var album = new Album
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            CoverUrl = imageUrl,
            ReleaseDate = DateTime.SpecifyKind(dto.ReleaseDate, DateTimeKind.Utc),
            ArtistId = dto.ArtistId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        return album.Id;
    }
}