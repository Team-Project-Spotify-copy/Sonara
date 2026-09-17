using Application.DTOs.Music;

namespace Application.Interfaces.Services;

public interface IAlbumService
{
    Task<IEnumerable<AlbumDto>> GetAllAsync(CancellationToken ct);
    Task<AlbumSummaryDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<AlbumSummaryDto?>> GetMyAlbumsAsync(Guid currentUserId, CancellationToken ct);
    Task<AlbumDto> CreateAsync(CreateAlbumDto dto, Guid currentUserId, CancellationToken ct);
    Task<AlbumDto?> UpdateAsync(Guid id, UpdateAlbumDto dto, Guid currentUserId, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct);
    Task<AlbumDto?> AddTrackToAlbumAsync(Guid albumId, string trackName, Guid currentUserId, CancellationToken ct);
}