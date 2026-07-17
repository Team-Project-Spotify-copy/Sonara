using System;
using Application.Entities.Music;

namespace Application.Entities.Playlists;

public class PlaylistTrack
{
    public Guid PlaylistId { get; set; }
    public Guid TrackId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    //Навігаційні властивості
    public virtual Playlist Playlist { get; set; } = null!;
    public virtual Track Track { get; set; } = null!;
}