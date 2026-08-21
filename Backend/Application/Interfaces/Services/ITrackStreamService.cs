using Application.DTOs.Music;

namespace Application.Interfaces.Services;

public interface ITrackStreamService
{
    Task<TrackStreamDto> ResolveAsync(Guid trackId, CancellationToken ct = default);
}
