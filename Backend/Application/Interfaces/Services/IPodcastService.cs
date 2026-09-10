using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Podcast;

namespace Application.Interfaces.Services;

public interface IPodcastService
{
    Task<IEnumerable<PodcastDto>> GetAllAsync();
    Task<IReadOnlyList<PodcastDto>> GetMyPodcastsAsync(Guid currentUserId, CancellationToken ct = default);
    Task<PodcastDetailsDto?> GetByIdAsync(Guid id);
    Task<PodcastDetailsDto?> GetByNameAsync(string podcastName);
    Task<PodcastDetailsDto?> AddEpisodeAsync(Guid id, string podcastEpisodeName, Guid currentUserId);
    Task<PodcastDto> CreateAsync(Guid authorId, CreatePodcastDto dto);
    Task<PodcastDto?> UpdateAsync(Guid id, Guid currentUserId, UpdatePodcastDto dto);
    Task<bool> DeleteAsync(Guid id, Guid currentUserId);
}