namespace Infrastructure.Services;

using Application.DTOs.Music;
using Application.Interfaces.Services;
using Domain.Entities.Music;
using Infrastructure.Data;

public class AdminMusicService : IAdminMusicService
{
    private readonly SonaraDbContext _context;

    public AdminMusicService(SonaraDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateTrackAsync(CreateTrackDto dto)
    {
        var track = new Track
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            DurationMs = (int)Math.Round(dto.DurationSeconds * 1000),
            AudioUrl = dto.AudioUrl,
            ArtistId = dto.ArtistId,
            AlbumId = dto.AlbumId,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.GenreIds.Any())
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
        var album = new Album
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            CoverUrl = dto.CoverUrl,
            ReleaseDate = dto.ReleaseDate.ToUniversalTime(),
            ArtistId = dto.ArtistId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        return album.Id;
    }
}