using Application.DTOs.Podcast;

namespace Application.Interfaces.Services;

public interface IPodcastService
{
    Task<IEnumerable<PodcastDto>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PodcastDto>> GetMyPodcastsAsync(Guid currentUserId, CancellationToken ct = default);
    Task<PodcastDetailsDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PodcastDetailsDto?> GetByNameAsync(string podcastName, CancellationToken ct = default);
    Task<PodcastDetailsDto?> AddEpisodeAsync(Guid id, string podcastEpisodeName, Guid currentUserId, CancellationToken ct = default);
    Task<PodcastDto> CreateAsync(Guid authorId, CreatePodcastDto dto, CancellationToken ct = default);
    Task<PodcastDto?> UpdateAsync(Guid id, Guid currentUserId, UpdatePodcastDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct = default);
}