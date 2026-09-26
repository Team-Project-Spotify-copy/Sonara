namespace Infrastructure.Services;

using Application.DTOs.Podcast;
using Application.Enums;
using Application.Interfaces;
using Application.Interfaces.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class AdminPodcastService : IAdminPodcastService
{
    private readonly SonaraDbContext _db;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;

    public AdminPodcastService(SonaraDbContext db, IBlobService blobService, IMapper mapper)
    {
        _db = db;
        _blobService = blobService;
        _mapper = mapper;
    }

    public async Task<PodcastDto> UpdatePodcastAsync(Guid id, UpdatePodcastDto dto)
    {
        var podcast = await _db.Podcasts.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Podcast with ID {id} was not found.");

        podcast.Title = dto.Title;
        podcast.Description = dto.Description;

        if (dto.CoverImage is { Length: > 0 })
        {
            podcast.CoverUrl = await _blobService.ReplaceFileAsync(dto.CoverImage, podcast.CoverUrl, BlobFolder.PodcastsCovers);
        }

        await _db.SaveChangesAsync();

        return _mapper.Map<PodcastDto>(podcast);
    }

    public async Task DeletePodcastAsync(Guid id)
    {
        var podcast = await _db.Podcasts.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Podcast with ID {id} was not found.");

        if (!string.IsNullOrWhiteSpace(podcast.CoverUrl))
        {
            await _blobService.DeleteFileAsync(podcast.CoverUrl, BlobFolder.PodcastsCovers);
        }

        _db.Podcasts.Remove(podcast);
        await _db.SaveChangesAsync();
    }
}