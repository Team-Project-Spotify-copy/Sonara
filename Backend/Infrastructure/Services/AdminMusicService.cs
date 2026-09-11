namespace Infrastructure.Services;

using Application.DTOs.Music;
using Application.Interfaces.Services;
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

    /// <summary>
    /// Maps genre names onto catalog rows, adding the ones that do not exist yet
    /// so a bulk source can supply tags without knowing catalog ids.
    /// </summary>
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

        // Artist.UserId is a required unique FK, so an artist cannot exist
        // without a User row. Imported artists get a placeholder account with an
        // unusable password hash - it carries the profile, it is not a login.
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

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            UserId = owner.Id,
            Name = name,
            Bio = dto.Bio,
            AvatarUrl = avatarUrl,
            Verified = false
        };

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
            // Backfill artwork for an album that was created without one.
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

        var album = new Album
        {
            Id = Guid.NewGuid(),
            ArtistId = dto.ArtistId,
            Title = title,
            CoverUrl = coverUrl,
            ReleaseDate = dto.ReleaseDate.HasValue
                ? DateTime.SpecifyKind(dto.ReleaseDate.Value, DateTimeKind.Utc)
                : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        return new ResolvedEntityDto { Id = album.Id, Name = album.Title, Created = true };
    }

    /// <summary>Never matches a real hash, so the placeholder cannot be signed into.</summary>
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

        // Usernames are not unique-constrained, but a suffix keeps imported
        // accounts distinguishable when two sources share a name.
        return $"{slug}_{Guid.NewGuid().ToString("N")[..6]}";
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
