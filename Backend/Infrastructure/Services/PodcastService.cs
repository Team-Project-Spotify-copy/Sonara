using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Podcast;
using Application.Enums;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities.Podcasts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class PodcastService : IPodcastService
    {
        private readonly SonaraDbContext _db;
        private readonly IBlobService _blobService;

        public PodcastService(SonaraDbContext context, IBlobService blobService)
        {
            _db = context;
            _blobService = blobService;
        }

        public async Task<IEnumerable<PodcastDto>> GetAllAsync()
        {
            return await _db.Podcasts
                .Include(p => p.Author)
                .Select(p => new PodcastDto
                {
                    Id = p.Id,
                    AuthorId = p.AuthorId,
                    Title = p.Title,
                    Description = p.Description,
                    CoverUrl = p.CoverUrl,
                    AuthorName = p.Author.Username
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<PodcastDto>> GetMyPodcastsAsync(Guid currentUserId, CancellationToken ct = default)
        {
            var podcasts = await _db.Podcasts
                .AsNoTracking()
                .Where(p => p.AuthorId == currentUserId) 
                .ToListAsync(ct);

            return podcasts.Select(p => new PodcastDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                CoverUrl = p.CoverUrl,
            }).ToList();
        }

        public async Task<PodcastDetailsDto?> GetByIdAsync(Guid id)
        {
            var podcast = await _db.Podcasts
                .Include(p => p.Author)
                .Include(p => p.Episodes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (podcast == null) return null;

            return new PodcastDetailsDto
            {
                Id = podcast.Id,
                AuthorId = podcast.AuthorId,
                Title = podcast.Title,
                Description = podcast.Description,
                CoverUrl = podcast.CoverUrl,
                AuthorName = podcast.Author.Username,
                AuthorAvatarUrl = podcast.Author.AvatarUrl,
                Episodes = podcast.Episodes.Select(e => new PodcastEpisodeDto
                {
                    Id = e.Id,
                    PodcastId = e.PodcastId,
                    Title = e.Title,
                    Description = e.Description,
                    AudioUrl = e.AudioUrl,
                    DurationMs = e.DurationMs,
                    ReleaseDate = e.ReleaseDate
                }).ToList()
            };
        }

        public async Task<PodcastDto> CreateAsync(Guid authorId, CreatePodcastDto dto)
        {
            string? coverUrl = null;

            // Якщо передано файл обкладинки — завантажуємо на Azure
            if (dto.CoverImage != null && dto.CoverImage.Length > 0)
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
            await _db.SaveChangesAsync();

            var author = await _db.Users.FindAsync(authorId);

            return new PodcastDto
            {
                Id = podcast.Id,
                AuthorId = podcast.AuthorId,
                Title = podcast.Title,
                Description = podcast.Description,
                CoverUrl = podcast.CoverUrl,
                AuthorName = author?.Username ?? string.Empty
            };
        }

        public async Task<PodcastDto?> UpdateAsync(Guid id, Guid currentUserId, UpdatePodcastDto dto)
        {
            var podcast = await _db.Podcasts.Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == id);
            if (podcast == null) return null;

            if (podcast.AuthorId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to update this podcast.");
            }

            podcast.Title = dto.Title;
            podcast.Description = dto.Description;

            // Якщо прикріплено нову картинку — замінюємо стару за допомогою ReplaceFileAsync
            if (dto.CoverImage != null && dto.CoverImage.Length > 0)
            {
                podcast.CoverUrl = await _blobService.ReplaceFileAsync(dto.CoverImage, podcast.CoverUrl, BlobFolder.PodcastsCovers);
            }

            await _db.SaveChangesAsync();

            return new PodcastDto
            {
                Id = podcast.Id,
                AuthorId = podcast.AuthorId,
                Title = podcast.Title,
                Description = podcast.Description,
                CoverUrl = podcast.CoverUrl,
                AuthorName = podcast.Author.Username
            };
        }

        public async Task<bool> DeleteAsync(Guid id, Guid currentUserId)
        {
            var podcast = await _db.Podcasts.FindAsync(id);
            if (podcast == null) return false;

            if (podcast.AuthorId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to delete this podcast.");
            }

            // Видаляємо обкладинку з Azure, якщо вона існує
            if (!string.IsNullOrWhiteSpace(podcast.CoverUrl))
            {
                await _blobService.DeleteFileAsync(podcast.CoverUrl, BlobFolder.PodcastsCovers);
            }

            _db.Podcasts.Remove(podcast);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}