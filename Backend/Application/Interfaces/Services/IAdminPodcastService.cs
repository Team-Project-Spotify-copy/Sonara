namespace Application.Interfaces.Services;

using Application.DTOs.Podcast;

public interface IAdminPodcastService
{
    Task<PodcastDto> UpdatePodcastAsync(Guid id, UpdatePodcastDto dto);
    Task DeletePodcastAsync(Guid id);
}