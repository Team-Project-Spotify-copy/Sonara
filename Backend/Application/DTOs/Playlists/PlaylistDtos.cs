namespace Application.DTOs.Playlists;

public record PlaylistDto(
    Guid Id,
    Guid UserId,
    string Name,
    string? Description,
    bool IsPrivate,
    string? CoverUrl,
    DateTime CreatedAt,
    int TracksCount);

public record PlaylistTrackDto(
    Guid TrackId,
    string Title,
    int DurationMs,
    string AudioUrl,
    DateTime AddedAt);

public record CreatePlaylistRequest(
    string Name,
    string? Description,
    bool IsPrivate,
    string? CoverUrl);

public record UpdatePlaylistRequest(
    string Name,
    string? Description,
    bool IsPrivate,
    string? CoverUrl);