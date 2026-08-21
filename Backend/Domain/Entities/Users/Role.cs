using System;
using System.Collections.Generic;

namespace Domain.Entities.Users;

public class Role : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
