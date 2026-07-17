using System;

namespace Domain.Entities.Podcasts;

public class PodcastEpisode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PodcastId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public int DurationMs { get; set; }
    public DateTime? ReleaseDate { get; set; }

    //Навігаційні властивості
    public virtual Podcast Podcast { get; set; } = null!;
}