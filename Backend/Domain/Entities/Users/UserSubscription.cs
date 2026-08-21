using Domain.Entities;
using Domain.Entities.Users;

public class UserSubscription : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlanId { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime ExpiresAt { get; set; }

    public virtual User Owner { get; set; } = null!;
    public virtual SubscriptionPlan Plan { get; set; } = null!;
    public virtual ICollection<User> Members { get; set; } = new List<User>();
}