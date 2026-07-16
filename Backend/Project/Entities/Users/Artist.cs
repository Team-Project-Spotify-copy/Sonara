using System;
using System.Collections.Generic;
using Domain.Entities.Music;

namespace Domain.Entities.Users;

public class Artist
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string? Bio { get; set; }
    public bool Verified { get; set; } = false;
//нафігаційні 'властивості 
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
}