using Application.DTOs.Music;
using Domain.Entities.Playlists;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

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

public record PlaylistTrackDto(
    int Position,
    DateTime AddedAt,
    TrackDto Track);

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
