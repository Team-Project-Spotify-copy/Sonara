using System;
using Application.Entities.Users;
using Application.Entities.Music;

namespace Application.Entities.Playlists;

public class LikedTrack
{
    public Guid UserId { get; set; }
    public Guid TrackId { get; set; }
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    //Навігаційні властивості
    public virtual User User { get; set; } = null!;
    public virtual Track Track { get; set; } = null!;
}