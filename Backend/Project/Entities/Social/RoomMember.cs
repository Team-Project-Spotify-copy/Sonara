using System;
using Domain.Entities.Users;

namespace Domain.Entities.Social;

public class RoomMember
{
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    //Навігаційні властивості
    public virtual ListeningRoom Room { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}