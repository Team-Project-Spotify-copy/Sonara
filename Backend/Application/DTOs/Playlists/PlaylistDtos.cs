using Application.DTOs.Music;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Playlists;

public record PlaylistDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string OwnerUsername { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public bool IsPrivate { get; init; }
    public string? CoverUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public int TracksCount { get; init; }
    public int TotalDurationMs { get; init; }
    public bool IsOwner { get; init; }

    public PlaylistDto()
    {
        OwnerUsername = string.Empty;
        Name = string.Empty;
    }

    public PlaylistDto(
        Guid id,
        Guid userId,
        string ownerUsername,
        string name,
        string? description,
        bool isPrivate,
        string? coverUrl,
        DateTime createdAt,
        int tracksCount,
        int totalDurationMs,
        bool isOwner)
    {
        Id = id;
        UserId = userId;
        OwnerUsername = ownerUsername;
        Name = name;
        Description = description;
        IsPrivate = isPrivate;
        CoverUrl = coverUrl;
        CreatedAt = createdAt;
        TracksCount = tracksCount;
        TotalDurationMs = totalDurationMs;
        IsOwner = isOwner;
    }
}

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
