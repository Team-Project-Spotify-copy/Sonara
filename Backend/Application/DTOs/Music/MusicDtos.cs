using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Music;

/// <summary>
/// Компактне подання треку для списків, карток та черги програвача.
/// Не містить фізичного посилання на медіа: відтворення йде через /api/tracks/{id}/stream.
/// </summary>
public class TrackDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public Guid ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;

    public Guid? AlbumId { get; set; }
    public string? AlbumTitle { get; set; }

    /// <summary>Обкладинка треку: обкладинка альбому, інакше аватар виконавця.</summary>
    public string? ArtworkUrl { get; set; }

    public int DurationMs { get; set; }

    /// <summary>Похідне значення для зручності плеєра (не зберігається в БД).</summary>
    public double DurationSeconds => Math.Round(DurationMs / 1000.0, 3);

    public List<string> Genres { get; set; } = new();

    public long PlaysCount { get; set; }

    /// <summary>true, якщо для треку існує медіа і /stream поверне посилання.</summary>
    public bool HasStream { get; set; }

    /// <summary>Стан "вподобано" для поточного користувача. false для анонімних запитів.</summary>
    public bool IsLiked { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Повне подання треку для сторінки треку. Розширює <see cref="TrackDto"/>.
/// </summary>
public class TrackDetailsDto : TrackDto
{
    public string? ArtistAvatarUrl { get; set; }
    public bool ArtistVerified { get; set; }

    public string? AlbumCoverUrl { get; set; }
    public string? AlbumType { get; set; }
    public DateTime? AlbumReleaseDate { get; set; }

    public long LikesCount { get; set; }
}

public class AlbumSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? Type { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public Guid ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public int TracksCount { get; set; }
}

public class AlbumDto : AlbumSummaryDto
{
    public int TotalDurationMs { get; set; }
    public List<TrackDto> Tracks { get; set; } = new();
}

public class ArtistSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool Verified { get; set; }
}

public class ArtistDto : ArtistSummaryDto
{
    public string? Bio { get; set; }
    public List<AlbumSummaryDto> Albums { get; set; } = new();
    public List<TrackDto> TopTracks { get; set; } = new();
}

public class PlaylistSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
    public int TracksCount { get; set; }
}

public class CreateTrackDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Range(0.1, 24 * 60 * 60)]
    public double DurationSeconds { get; set; }

    [Required]
    public IFormFile AudioFile { get; set; } = null!;

    [Required]
    public Guid ArtistId { get; set; }

    public Guid? AlbumId { get; set; }

    public List<Guid> GenreIds { get; set; } = new();
}

public class CreateAlbumDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public IFormFile CoverImage { get; set; } = null!;

    public DateTime ReleaseDate { get; set; }

    [Required]
    public Guid ArtistId { get; set; }
}
