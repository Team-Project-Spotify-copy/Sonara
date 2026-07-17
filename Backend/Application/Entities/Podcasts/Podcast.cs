using System;
using System.Collections.Generic;
using Domain.Entities.Users;

namespace Domain.Entities.Podcasts;

public class Podcast
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }

    //Навігаційні властивості
    public virtual User Author { get; set; } = null!;
    public virtual ICollection<PodcastEpisode> Episodes { get; set; } = new List<PodcastEpisode>();
}