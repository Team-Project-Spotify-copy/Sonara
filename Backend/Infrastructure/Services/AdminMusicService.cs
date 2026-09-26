namespace Infrastructure.Services;

using Application.DTOs.Music;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities.Music;
using Domain.Entities.Users;
using Infrastructure.Data;
using Application.Enums;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AdminMusicService : IAdminMusicService
{
    private readonly SonaraDbContext _context;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;

    public AdminMusicService(SonaraDbContext context, IBlobService blobService, IMapper mapper)
    {
        _context = context;
        _blobService = blobService;
        _mapper = mapper;
    }

    public async Task<Guid> CreateTrackAsync(CreateTrackDto dto)
    {
        if (dto.AudioFile == null || dto.AudioFile.Length == 0)
        {
            throw new ArgumentException("File not found");
        }

        string audioUrl = await _blobService.UploadFileAsync(dto.AudioFile, BlobFolder.MusicTracks);

        var track = _mapper.Map<Track>(dto);
        track.AudioUrl = audioUrl;

        var genreIds = new List<Guid>(dto.GenreIds ?? new List<Guid>());

        if (dto.GenreNames != null && dto.GenreNames.Any())
        {
            genreIds.AddRange(await ResolveGenreIdsAsync(dto.GenreNames));
        }

        var distinctGenreIds = genreIds.Distinct().ToList();

        if (distinctGenreIds.Any())
        {
            track.TrackGenres = distinctGenreIds.Select(genreId => new TrackGenre
            {
                TrackId = track.Id,
                GenreId = genreId
            }).ToList();
        }

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        return track.Id;
    }

    private async Task<List<Guid>> ResolveGenreIdsAsync(IEnumerable<string> names)
    {
        var wanted = names
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Where(n => n.Length <= 100)
            .GroupBy(n => n.ToLowerInvariant())
            .Select(g => g.First())
            .ToList();

        if (wanted.Count == 0) return new List<Guid>();

        var lowered = wanted.Select(n => n.ToLowerInvariant()).ToList();

        var existing = await _context.Genres
            .Where(g => lowered.Contains(g.Name.ToLower()))
            .ToListAsync();

        var resolved = new List<Guid>();

        foreach (var name in wanted)
        {
            var match = existing.FirstOrDefault(g =>
                string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                match = new Genre { Id = Guid.NewGuid(), Name = name };
                _context.Genres.Add(match);
                existing.Add(match);
            }

            resolved.Add(match.Id);
        }

        return resolved;
    }

    public async Task<ResolvedEntityDto> ResolveArtistAsync(ResolveArtistDto dto)
    {
        var name = (dto.Name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Artist name is required");

        var existing = await _context.Artists
            .FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower());

        if (existing is not null)
        {
            return new ResolvedEntityDto { Id = existing.Id, Name = existing.Name, Created = false };
        }

        string? avatarUrl = dto.AvatarUrl;
        if (dto.AvatarImage is { Length: > 0 })
        {
            avatarUrl = await _blobService.UploadFileAsync(dto.AvatarImage, BlobFolder.Avatars);
        }

        var owner = new User
        {
            Id = Guid.NewGuid(),
            Username = BuildImportedUsername(name),
            Email = $"{BuildImportedUsername(name)}@imported.invalid",
            PasswordHash = ImportedAccountPasswordHash,
            AvatarUrl = avatarUrl,
            RoleId = await ResolveListenerRoleIdAsync(),
            CreatedAt = DateTime.UtcNow
        };

        var artist = _mapper.Map<Artist>(dto);
        artist.UserId = owner.Id;
        artist.AvatarUrl = avatarUrl;

        _context.Users.Add(owner);
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        return new ResolvedEntityDto { Id = artist.Id, Name = artist.Name, Created = true };
    }

    public async Task<ResolvedEntityDto> ResolveAlbumAsync(ResolveAlbumDto dto)
    {
        var title = (dto.Title ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Album title is required");

        var artistExists = await _context.Artists.AnyAsync(a => a.Id == dto.ArtistId);
        if (!artistExists)
            throw new ArgumentException("Unknown artist");

        var existing = await _context.Albums
            .FirstOrDefaultAsync(a => a.ArtistId == dto.ArtistId && a.Title.ToLower() == title.ToLower());

        if (existing is not null)
        {
            if (string.IsNullOrWhiteSpace(existing.CoverUrl))
            {
                if (dto.CoverImage is { Length: > 0 })
                {
                    existing.CoverUrl = await _blobService.UploadFileAsync(dto.CoverImage, BlobFolder.AlbumsCovers);
                    await _context.SaveChangesAsync();
                }
                else if (!string.IsNullOrWhiteSpace(dto.CoverUrl))
                {
                    existing.CoverUrl = dto.CoverUrl;
                    await _context.SaveChangesAsync();
                }
            }

            return new ResolvedEntityDto { Id = existing.Id, Name = existing.Title, Created = false };
        }

        string? coverUrl = dto.CoverUrl;
        if (dto.CoverImage is { Length: > 0 })
        {
            coverUrl = await _blobService.UploadFileAsync(dto.CoverImage, BlobFolder.AlbumsCovers);
        }

        var album = _mapper.Map<Album>(dto);
        album.CoverUrl = coverUrl;

        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        return new ResolvedEntityDto { Id = album.Id, Name = album.Title, Created = true };
    }

    private const string ImportedAccountPasswordHash = "imported-account-no-login";

    private async Task<Guid> ResolveListenerRoleIdAsync()
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User")
            ?? await _context.Roles.OrderBy(r => r.Name).FirstOrDefaultAsync();

        if (role is null)
            throw new InvalidOperationException("No role exists to attach the imported artist account to.");

        return role.Id;
    }

    private static string BuildImportedUsername(string name)
    {
        var slug = new string(name.ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '_')
            .ToArray())
            .Trim('_');

        if (slug.Length > 40) slug = slug[..40];
        if (string.IsNullOrEmpty(slug)) slug = "artist";

        return $"{slug}_{Guid.NewGuid().ToString("N")[..6]}";
    }

    public async Task<Guid> CreateAlbumAsync(CreateAlbumDto dto)
    {
        if (dto.CoverImage == null || dto.CoverImage.Length == 0)
        {
            throw new ArgumentException("File not found");
        }

        string imageUrl = await _blobService.UploadFileAsync(dto.CoverImage, BlobFolder.AlbumsCovers);

        var album = _mapper.Map<Album>(dto);
        album.CoverUrl = imageUrl;

        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        return album.Id;
    }

    public async Task<TrackDto> UpdateTrackAsync(Guid id, UpdateTrackDto dto)
    {
        var track = await _context.Tracks
            .Include(t => t.TrackGenres)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Track with ID {id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.Title))
            track.Title = dto.Title;

        if (dto.ArtistId.HasValue)
        {
            var artistExists = await _context.Artists.AnyAsync(a => a.Id == dto.ArtistId.Value);
            if (!artistExists)
                throw new ArgumentException("Unknown artist");

            track.ArtistId = dto.ArtistId.Value;
        }

        if (dto.AlbumId.HasValue)
            track.AlbumId = dto.AlbumId.Value;

        if (dto.AudioFile is { Length: > 0 })
        {
            track.AudioUrl = await _blobService.ReplaceFileAsync(dto.AudioFile, track.AudioUrl, BlobFolder.MusicTracks);
        }

        if (dto.GenreIds is not null)
        {
            _context.TrackGenres.RemoveRange(track.TrackGenres);
            track.TrackGenres = dto.GenreIds.Distinct()
                .Select(genreId => new TrackGenre { TrackId = track.Id, GenreId = genreId })
                .ToList();
        }

        await _context.SaveChangesAsync();

        // Перечитуємо з бази, щоб DTO віддав актуальні Artist/Album/Genre назви.
        var reloaded = await _context.Tracks
            .AsNoTracking()
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.TrackGenres).ThenInclude(tg => tg.Genre)
            .FirstAsync(t => t.Id == id);

        return new TrackDto
        {
            Id = reloaded.Id,
            Title = reloaded.Title,
            ArtistId = reloaded.ArtistId,
            ArtistName = reloaded.Artist.Name,
            AlbumId = reloaded.AlbumId,
            AlbumTitle = reloaded.Album?.Title,
            ArtworkUrl = reloaded.Album?.CoverUrl,
            DurationMs = reloaded.DurationMs,
            Genres = reloaded.TrackGenres.Select(tg => tg.Genre.Name).ToList(),
            PlaysCount = reloaded.PlaysCount,
            HasStream = !string.IsNullOrWhiteSpace(reloaded.AudioUrl),
            CreatedAt = reloaded.CreatedAt
        };
    }

    public async Task DeleteTrackAsync(Guid id)
    {
        var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Track with ID {id} was not found.");

        if (!string.IsNullOrWhiteSpace(track.AudioUrl))
        {
            await _blobService.DeleteFileAsync(track.AudioUrl, BlobFolder.MusicTracks);
        }

        _context.Tracks.Remove(track);
        await _context.SaveChangesAsync();
    }

    public async Task<AlbumDto> UpdateAlbumAsync(Guid id, UpdateAlbumDto dto)
    {
        // На відміну від AlbumService.UpdateAsync (для артиста-власника), тут
        // НЕМАЄ перевірки власника — адмін може редагувати будь-який альбом.
        var album = await _context.Albums
            .Include(a => a.Artist)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Album with ID {id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.Title))
            album.Title = dto.Title;

        if (dto.ReleaseDate.HasValue)
            album.ReleaseDate = dto.ReleaseDate.Value;

        if (dto.CoverImage is { Length: > 0 })
        {
            album.CoverUrl = await _blobService.ReplaceFileAsync(dto.CoverImage, album.CoverUrl, BlobFolder.AlbumsCovers);
        }

        await _context.SaveChangesAsync();

        return _mapper.Map<AlbumDto>(album);
    }

    public async Task DeleteAlbumAsync(Guid id)
    {
        var album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Album with ID {id} was not found.");

        if (!string.IsNullOrWhiteSpace(album.CoverUrl))
        {
            await _blobService.DeleteFileAsync(album.CoverUrl, BlobFolder.AlbumsCovers);
        }

        _context.Albums.Remove(album);
        await _context.SaveChangesAsync();
    }
}