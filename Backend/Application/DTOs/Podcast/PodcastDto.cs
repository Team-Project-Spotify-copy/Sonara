namespace Application.DTOs.Podcast;

public class PodcastDto
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public string AuthorName { get; set; } = string.Empty;
}
public class PodcastDetailsDto : PodcastDto
{
    public string? AuthorAvatarUrl { get; set; }
    public ICollection<PodcastEpisodeDto> Episodes { get; set; } = new List<PodcastEpisodeDto>();
}