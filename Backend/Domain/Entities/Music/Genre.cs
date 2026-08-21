using System;
using System.Collections.Generic;

namespace Domain.Entities.Music;

public class Genre : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<TrackGenre> TrackGenres { get; set; } = new List<TrackGenre>();
}
