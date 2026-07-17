using System;
using Domain.Entities.Users;

namespace Domain.Entities.Music;

public class ListeningHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid TrackId { get; set; }
    public DateTime ListenedAt { get; set; } = DateTime.UtcNow;
    public int? DurationListenedMs { get; set; }

    //Навігаційні властивості
    public virtual User User { get; set; } = null!;
    public virtual Track Track { get; set; } = null!;
}