using Application.DTOs.Music;
using Application.Enums;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Music;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AlbumService : IAlbumService
{
    private readonly SonaraDbContext _db;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;

    public AlbumService(SonaraDbContext db, IBlobService blobService, IMapper mapper)
    {
        _db = db;
        _blobService = blobService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AlbumDto>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Albums
            .AsNoTracking()
            .ProjectTo<AlbumDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<AlbumSummaryDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Albums
            .AsNoTracking()
            .Where(a => a.Id == id)
            .ProjectTo<AlbumSummaryDto>(_mapper.ConfigurationProvider)
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
            throw new NotFoundException("artist profile", currentUserId);

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

        return await GetAlbumDtoByIdAsync(album.Id, ct) ?? throw new InvalidOperationException("Failed to create album.");
    }

    public async Task<AlbumDto?> UpdateAsync(Guid id, UpdateAlbumDto dto, Guid currentUserId, CancellationToken ct)
    {
        var album = await _db.Albums
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (album == null) return null;

        if (album.Artist?.UserId != currentUserId)
            throw new UnauthorizedAccessException("You are not allowed to update this album.");

        if (dto.Title != null) album.Title = dto.Title;
        if (dto.ReleaseDate.HasValue) album.ReleaseDate = dto.ReleaseDate.Value;

        if (dto.CoverImage != null)
        {
            album.CoverUrl = await _blobService.ReplaceFileAsync(dto.CoverImage, album.CoverUrl, BlobFolder.AlbumsCovers);
        }

        await _db.SaveChangesAsync(ct);

        return _mapper.Map<AlbumDto>(album);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct)
    {
        var album = await _db.Albums
            .Include(a => a.Artist)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (album == null) return false;

        if (album.Artist?.UserId != currentUserId)
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
            .ThenInclude(t => t.Artist)
            .FirstOrDefaultAsync(a => a.Id == albumId, ct);

        if (album == null) return null;

        if (album.Artist?.UserId != currentUserId)
            throw new UnauthorizedAccessException("You are not allowed to modify this album.");

        var track = await _db.Tracks
            .FirstOrDefaultAsync(t => t.Title == trackName, ct);

        if (track == null) return null;

        track.AlbumId = album.Id;

        await _db.SaveChangesAsync(ct);

        return _mapper.Map<AlbumDto>(album);
    }

    private async Task<AlbumDto?> GetAlbumDtoByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Albums
            .AsNoTracking()
            .Where(a => a.Id == id)
            .ProjectTo<AlbumDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Album>> GetMyAlbumsAsync(Guid currentUserId, CancellationToken ct)
    {
        return await _db.Albums
                .AsNoTracking()
                .Include(a => a.Artist)
                .Include(a => a.Tracks)
                .Where(a => a.Artist.UserId == currentUserId)
                .ToListAsync(ct);
    }
}