namespace Infrastructure.Services;

using Application.DTOs.Music;
using Application.Exceptions;
using Application.Helpers;
using Application.Interfaces.Services;
using Domain.Entities.Music;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class MusicCatalogService : IMusicCatalogService
{
    private readonly SonaraDbContext _context;

    public MusicCatalogService(SonaraDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TrackDto>> GetTracksAsync(int pageNumber, int pageSize, string? genre)
    {
        var query = _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.TrackGenres)
                .ThenInclude(tg => tg.Genre)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(t => t.TrackGenres.Any(tg => tg.Genre.Name.ToLower() == genre.ToLower()));
        }

        var count = await query.CountAsync();

        var tracks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                DurationSeconds = t.DurationMs / 1000.0,
                AudioUrl = t.AudioUrl,
                ArtistId = t.ArtistId,
                ArtistName = t.Artist.Name,
                AlbumId = t.AlbumId,
                AlbumTitle = t.Album != null ? t.Album.Title : null,
                Genres = t.TrackGenres.Select(tg => tg.Genre.Name).ToList()
            })
            .ToListAsync();

        return new PaginatedList<TrackDto>(tracks, count, pageNumber, pageSize);
    }

    public async Task<AlbumDto> GetAlbumByIdAsync(Guid id)
    {
        var album = await _context.Albums
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException(nameof(Album), id);

        return new AlbumDto
        {
            Id = album.Id,
            Title = album.Title,
            CoverUrl = album.CoverUrl ?? string.Empty,
            ReleaseDate = album.ReleaseDate,
            ArtistId = album.ArtistId,
            ArtistName = album.Artist.Name,
            Tracks = album.Tracks.Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                DurationSeconds = t.DurationMs / 1000.0,
                AudioUrl = t.AudioUrl,
                ArtistId = t.ArtistId,
                ArtistName = album.Artist.Name,
                AlbumId = album.Id,
                AlbumTitle = album.Title
            }).ToList()
        };
    }

    public async Task<ArtistDto> GetArtistByIdAsync(Guid id)
    {
        var artist = await _context.Artists
            .Include(a => a.Albums)
            .Include(a => a.Tracks.OrderByDescending(t => t.PlaysCount).Take(10))
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException("Artist", id);

        return new ArtistDto
        {
            Id = artist.Id,
            Name = artist.Name,
            Bio = artist.Bio,
            AvatarUrl = artist.AvatarUrl ?? string.Empty,
            Albums = artist.Albums.Select(a => new AlbumDto
            {
                Id = a.Id,
                Title = a.Title,
                CoverUrl = a.CoverUrl ?? string.Empty,
                ReleaseDate = a.ReleaseDate
            }).ToList(),
            TopTracks = artist.Tracks.OrderByDescending(t => t.PlaysCount).Select(t => new TrackDto
            {
                Id = t.Id,
                Title = t.Title,
                DurationSeconds = t.DurationMs / 1000.0,
                AudioUrl = t.AudioUrl,
                ArtistId = artist.Id,
                ArtistName = artist.Name,
                AlbumId = t.AlbumId
            }).ToList()
        };
    }

    public async Task<SearchResultDto> SearchAsync(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new SearchResultDto();

        var normalizedQuery = $"%{query}%";

        // Запити виконуються послідовно: DbContext не є потокобезпечним
        // і не підтримує кілька одночасних операцій.
        var tracks = await _context.Tracks
            .AsNoTracking()
            .Where(t => EF.Functions.ILike(t.Title, normalizedQuery))
            .Take(limit)
            .Select(t => new TrackDto { Id = t.Id, Title = t.Title, ArtistName = t.Artist.Name })
            .ToListAsync();

        var albums = await _context.Albums
            .AsNoTracking()
            .Where(a => EF.Functions.ILike(a.Title, normalizedQuery))
            .Take(limit)
            .Select(a => new AlbumDto { Id = a.Id, Title = a.Title, ArtistName = a.Artist.Name })
            .ToListAsync();

        var artists = await _context.Artists
            .AsNoTracking()
            .Where(a => EF.Functions.ILike(a.Name, normalizedQuery))
            .Take(limit)
            .Select(a => new ArtistDto { Id = a.Id, Name = a.Name })
            .ToListAsync();

        return new SearchResultDto
        {
            Tracks = tracks,
            Albums = albums,
            Artists = artists
        };
    }
}