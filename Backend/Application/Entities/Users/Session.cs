using System;

namespace Application.Entities.Users;

public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DeviceName { get; set; } = string.Empty; 
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

    // Зв'язок із Користувачем
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
}