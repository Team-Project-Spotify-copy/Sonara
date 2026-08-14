using Application.DTOs.Music;
using Application.DTOs.Playlists;

namespace Application.Interfaces.Services;

public interface IPlaylistService
{
    Task<IReadOnlyList<PlaylistDto>> GetMyPlaylistsAsync(Guid currentUserId, CancellationToken ct = default);

    /// <summary>Private playlists are visible only to their owner; otherwise ForbiddenAccessException.</summary>
    Task<PlaylistDto> GetByIdAsync(Guid playlistId, Guid? requestingUserId, CancellationToken ct = default);

    Task<PlaylistDto> CreateAsync(Guid ownerId, CreatePlaylistRequest request, CancellationToken ct = default);

    Task<PlaylistDto> UpdateAsync(Guid playlistId, Guid ownerId, UpdatePlaylistRequest request, CancellationToken ct = default);

    Task DeleteAsync(Guid playlistId, Guid ownerId, CancellationToken ct = default);

    Task<IReadOnlyList<PlaylistTrackDto>> GetTracksAsync(Guid playlistId, Guid? requestingUserId, CancellationToken ct = default);

    /// <summary>Idempotent: adding an already present track is a no-op. Returns the updated playlist.</summary>
    Task<PlaylistDto> AddTrackAsync(Guid playlistId, Guid ownerId, Guid trackId, CancellationToken ct = default);

    /// <summary>Idempotent: removing an absent track is a no-op. Returns the updated playlist.</summary>
    Task<PlaylistDto> RemoveTrackAsync(Guid playlistId, Guid ownerId, Guid trackId, CancellationToken ct = default);
}
