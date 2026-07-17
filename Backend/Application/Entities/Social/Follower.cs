using System;
using Application.Entities.Users;

namespace Application.Entities.Social;

public class Follower
{
    public Guid FollowerId { get; set; }
    public Guid FollowedId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //Навігаційні властивості
    public virtual User FollowerUser { get; set; } = null!;
    public virtual User FollowedUser { get; set; } = null!;
}