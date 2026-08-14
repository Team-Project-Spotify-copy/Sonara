using System.ComponentModel.DataAnnotations;
using Application.DTOs.Music;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Playlists;

public record PlaylistDto(
    Guid Id,
    Guid UserId,
    string OwnerUsername,
    string Name,
    string? Description,
    bool IsPrivate,
    string? CoverUrl,
    DateTime CreatedAt,
    int TracksCount,
    int TotalDurationMs,
    bool IsOwner);

/// <summary>Позиція треку в плейлісті разом із повними даними треку для плеєра.</summary>
public record PlaylistTrackDto(
    int Position,
    DateTime AddedAt,
    TrackDto Track);

// Для record-типів MVC вимагає, щоб атрибути валідації стояли на ПАРАМЕТРІ первинного
// конструктора, а не на властивості: інакше запит падає з 500 замість 400.

public record CreatePlaylistRequest(
    [Required]
    [StringLength(100, MinimumLength = 1)]
    string Name,

    [StringLength(500)]
    string? Description,

    bool IsPrivate,

    IFormFile? CoverImage);

public record UpdatePlaylistRequest(
    [Required]
    [StringLength(100, MinimumLength = 1)]
    string Name,

    [StringLength(500)]
    string? Description,

    bool IsPrivate,

    IFormFile? CoverImage);
