using Microsoft.AspNetCore.Http;
using Application.DTOs.Music;
using Application.DTOs.Playlists;

namespace Application.DTOs.Users;

public record ProfileDto
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int CountPlaylist { get; set; }
    public int CountFollowers { get; set; }
    public bool? IsFollowing { get; set; }
    public IList<PlaylistDto>? Playlists { get; set; }
    public IList<ListeningHistoryEntryDto>? History { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public record UpdateProfileDto
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public IFormFile? AvatarFile { get; set; } = null;
}