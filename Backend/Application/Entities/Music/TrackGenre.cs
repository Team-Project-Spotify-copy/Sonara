using System;

namespace Application.Entities.Music;

public class TrackGenre
{
    public Guid TrackId { get; set; }
    public Guid GenreId { get; set; }

    //Навігаційні властивості
    public virtual Track Track { get; set; } = null!;
    public virtual Genre Genre { get; set; } = null!;
}