using System;

namespace Application.DTOs.Podcast;

public class PodcastEpisodeDto
{
    public Guid Id { get; set; }
    public Guid PodcastId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public int DurationMs { get; set; }
    public DateTime? ReleaseDate { get; set; }
}