using System;
using Domain.Entities.Users;

namespace Domain.Entities.Social;

public class Follower
{
    public Guid FollowerId { get; set; }
    public Guid FollowedId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //Навігаційні властивості
    public virtual User FollowerUser { get; set; } = null!;
    public virtual User FollowedUser { get; set; } = null!;
}