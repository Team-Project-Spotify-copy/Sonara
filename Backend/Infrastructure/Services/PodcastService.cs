namespace Infrastructure.Services;

using Application.DTOs.Podcast;
using Application.Enums;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Podcasts;
using Microsoft.EntityFrameworkCore;

public class PodcastService : IPodcastService
{
    private readonly SonaraDbContext _db;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;

    public PodcastService(SonaraDbContext context, IBlobService blobService, IMapper mapper)
    {
        _db = context;
        _blobService = blobService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PodcastDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Podcasts
            .AsNoTracking()
            .ProjectTo<PodcastDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PodcastDto>> GetMyPodcastsAsync(Guid currentUserId, CancellationToken ct = default)
    {
        return await _db.Podcasts
            .AsNoTracking()
            .Where(p => p.AuthorId == currentUserId)
            .ProjectTo<PodcastDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<PodcastDetailsDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Podcasts
            .AsNoTracking()
            .Where(p => p.Id == id)
            .ProjectTo<PodcastDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PodcastDetailsDto?> GetByNameAsync(string podcastName, CancellationToken ct = default)
    {
        return await _db.Podcasts
            .AsNoTracking()
            .Where(p => p.Title == podcastName)
            .ProjectTo<PodcastDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PodcastDto> CreateAsync(Guid authorId, CreatePodcastDto dto, CancellationToken ct = default)
    {
        string? coverUrl = null;

        if (dto.CoverImage is { Length: > 0 })
        {
            coverUrl = await _blobService.UploadFileAsync(dto.CoverImage, BlobFolder.PodcastsCovers);
        }

        var podcast = new Podcast
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId,
            Title = dto.Title,
            Description = dto.Description,
            CoverUrl = coverUrl
        };

        _db.Podcasts.Add(podcast);
        await _db.SaveChangesAsync(ct);

        return await _db.Podcasts
            .AsNoTracking()
            .Where(p => p.Id == podcast.Id)
            .ProjectTo<PodcastDto>(_mapper.ConfigurationProvider)
            .FirstAsync(ct);
    }

    public async Task<PodcastDto?> UpdateAsync(Guid id, Guid currentUserId, UpdatePodcastDto dto, CancellationToken ct = default)
    {
        var podcast = await _db.Podcasts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (podcast == null) return null;

        if (podcast.AuthorId != currentUserId)
        {
            throw new ForbiddenAccessException("You are not allowed to update this podcast.");
        }

        podcast.Title = dto.Title;
        podcast.Description = dto.Description;

        if (dto.CoverImage is { Length: > 0 })
        {
            podcast.CoverUrl = await _blobService.ReplaceFileAsync(dto.CoverImage, podcast.CoverUrl, BlobFolder.PodcastsCovers);
        }

        await _db.SaveChangesAsync(ct);

        return _mapper.Map<PodcastDto>(podcast);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct = default)
    {
        var podcast = await _db.Podcasts.FindAsync(new object[] { id }, ct);
        if (podcast == null) return false;

        if (podcast.AuthorId != currentUserId)
        {
            throw new ForbiddenAccessException("You are not allowed to delete this podcast.");
        }

        if (!string.IsNullOrWhiteSpace(podcast.CoverUrl))
        {
            await _blobService.DeleteFileAsync(podcast.CoverUrl, BlobFolder.PodcastsCovers);
        }

        _db.Podcasts.Remove(podcast);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PodcastDetailsDto?> AddEpisodeAsync(Guid id, string podcastEpisodeName, Guid currentUserId, CancellationToken ct = default)
    {
        var podcast = await _db.Podcasts
            .Include(p => p.Author)
            .Include(p => p.Episodes)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (podcast == null) return null;

        if (podcast.AuthorId != currentUserId)
        {
            throw new ForbiddenAccessException("You are not allowed to modify this podcast.");
        }

        var episode = await _db.PodcastEpisodes
            .FirstOrDefaultAsync(e => e.Title == podcastEpisodeName, ct);

        if (episode == null) return null;

        if (!podcast.Episodes.Any(e => e.Id == episode.Id))
        {
            podcast.Episodes.Add(episode);
            await _db.SaveChangesAsync(ct);
        }

        return _mapper.Map<PodcastDetailsDto>(podcast);
    }
}