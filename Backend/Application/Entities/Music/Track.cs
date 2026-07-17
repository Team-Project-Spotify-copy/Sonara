using System;
using System.Collections.Generic;
using Application.Entities.Playlists;

namespace Application.Entities.Music;

public class Track
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? AlbumId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DurationMs { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public long PlaysCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //Навігаційні властивості
    public virtual Album? Album { get; set; }
    public virtual ICollection<TrackGenre> TrackGenres { get; set; } = new List<TrackGenre>();
    public virtual ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
    public virtual ICollection<LikedTrack> LikedByUsers { get; set; } = new List<LikedTrack>();
    public virtual ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();
}