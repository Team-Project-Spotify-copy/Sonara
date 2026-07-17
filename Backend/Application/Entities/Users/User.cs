using System;
using System.Collections.Generic;
using Application.Entities.Music;
using Application.Entities.Playlists;
using Application.Entities.Podcasts;
using Application.Entities.Social;

namespace Application.Entities.Users;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Зв'язок із Роллю
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    // Зв'язок із Підпискою
    public Guid SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;

    // Навігаційні властивості безпеки та сесій
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    // Навігаційні властивості контенту та соціальної взаємодії
    public virtual Artist? ArtistProfile { get; set; }
    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
    public virtual ICollection<LikedTrack> LikedTracks { get; set; } = new List<LikedTrack>();
    public virtual ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();
    public virtual ICollection<Follower> Followers { get; set; } = new List<Follower>(); 
    public virtual ICollection<Follower> Following { get; set; } = new List<Follower>(); 
    public virtual ICollection<Podcast> Podcasts { get; set; } = new List<Podcast>();
    public virtual ICollection<ListeningRoom> HostedRooms { get; set; } = new List<ListeningRoom>();
    public virtual ICollection<RoomMember> JoinedRooms { get; set; } = new List<RoomMember>();
}