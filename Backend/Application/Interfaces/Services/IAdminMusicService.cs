namespace Application.Interfaces.Services;

using Application.DTOs.Music;

public interface IAdminMusicService
{
    Task<Guid> CreateTrackAsync(CreateTrackDto dto);
    Task<Guid> CreateAlbumAsync(CreateAlbumDto dto);
}