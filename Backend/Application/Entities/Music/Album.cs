using System;
using System.Collections.Generic;
using Application.Entities.Users;

namespace Application.Entities.Music;

public class Album
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ArtistId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? ReleaseDate { get; set; }
    public string? CoverUrl { get; set; }
    public string? Type { get; set; }

    //Навігаційні властивості
    public virtual Artist Artist { get; set; } = null!;
    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}