using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Music;

public class TrackDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public double DurationSeconds { get; set; }
    public string AudioUrl { get; set; } = string.Empty; // URL з Azure Blob/CDN
    public Guid ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public Guid? AlbumId { get; set; }
    public string? AlbumTitle { get; set; }
    public List<string> Genres { get; set; } = new();
}

public class AlbumDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public DateTime? ReleaseDate { get; set; }
    public Guid ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public List<TrackDto> Tracks { get; set; } = new();
}

public class ArtistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public List<AlbumDto> Albums { get; set; } = new();
    public List<TrackDto> TopTracks { get; set; } = new();
}

public class SearchResultDto
{
    public List<TrackDto> Tracks { get; set; } = new();
    public List<AlbumDto> Albums { get; set; } = new();
    public List<ArtistDto> Artists { get; set; } = new();
}

public class CreateTrackDto
{
    public string Title { get; set; } = string.Empty;
    public double DurationSeconds { get; set; }
    public IFormFile AudioFile { get; set; } = null!;
    public Guid ArtistId { get; set; }
    public Guid? AlbumId { get; set; }
    public List<Guid> GenreIds { get; set; } = new();
}

public class CreateAlbumDto
{
    public string Title { get; set; } = string.Empty;
    public IFormFile CoverImage { get; set; } = null!;
    public DateTime ReleaseDate { get; set; }
    public Guid ArtistId { get; set; }
}