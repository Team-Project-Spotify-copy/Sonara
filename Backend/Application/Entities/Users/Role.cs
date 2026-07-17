using System;
using System.Collections.Generic;

namespace Application.Entities.Users;

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty; // "User", "Artist", "Admin"

    // Навігаційні властивості
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}