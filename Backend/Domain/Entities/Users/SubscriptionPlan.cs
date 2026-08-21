namespace Domain.Entities.Users;

public class SubscriptionPlan : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Features { get; set; } = string.Empty;
    public int MaxSlots { get; set; } 
}