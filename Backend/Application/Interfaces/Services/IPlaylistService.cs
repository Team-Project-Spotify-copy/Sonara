using Application.DTOs.Playlists;

namespace Application.Interfaces.Services;

public interface IPlaylistService
{
    Task<IReadOnlyList<PlaylistDto>> GetMyPlaylistsAsync(Guid currentUserId, CancellationToken ct = default);

    Task<PlaylistDto> GetByIdAsync(Guid playlistId, Guid? requestingUserId, CancellationToken ct = default);

    Task<PlaylistDto> CreateAsync(Guid ownerId, CreatePlaylistRequest request, CancellationToken ct = default);

    Task<PlaylistDto> UpdateAsync(Guid playlistId, Guid ownerId, UpdatePlaylistRequest request, CancellationToken ct = default);

    Task DeleteAsync(Guid playlistId, Guid ownerId, CancellationToken ct = default);

    Task<IReadOnlyList<PlaylistTrackDto>> GetTracksAsync(Guid playlistId, Guid? requestingUserId, CancellationToken ct = default);

    Task AddTrackAsync(Guid playlistId, Guid ownerId, Guid trackId, CancellationToken ct = default);

    Task RemoveTrackAsync(Guid playlistId, Guid ownerId, Guid trackId, CancellationToken ct = default);
}