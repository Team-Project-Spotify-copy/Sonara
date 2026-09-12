namespace Application.Interfaces.Services;

using Application.DTOs.Music;

public interface IAdminMusicService
{
    Task<Guid> CreateTrackAsync(CreateTrackDto dto);
    Task<Guid> CreateAlbumAsync(CreateAlbumDto dto);

    /// <summary>Returns the artist with this name, creating it when absent.</summary>
    Task<ResolvedEntityDto> ResolveArtistAsync(ResolveArtistDto dto);

    /// <summary>Returns the album with this artist + title, creating it when absent.</summary>
    Task<ResolvedEntityDto> ResolveAlbumAsync(ResolveAlbumDto dto);
}