using System;
using System.Collections.Generic;
using Application.Entities.Users;
using Application.Entities.Music;

namespace Application.Entities.Social;

public class ListeningRoom
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HostId { get; set; }
    public string? Name { get; set; }
    public Guid? CurrentTrackId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //Навігаційні властивості
    public virtual User Host { get; set; } = null!;
    public virtual Track? CurrentTrack { get; set; }
    public virtual ICollection<RoomMember> Members { get; set; } = new List<RoomMember>();
}