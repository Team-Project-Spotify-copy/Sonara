using System;
using System.Collections.Generic;

namespace Domain.Entities.Users;

public class Subscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty; // "Free", "Premium", "Family"
    public decimal Price { get; set; }
    public string Features { get; set; } = string.Empty; 

    // Навігаційні властивості
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}