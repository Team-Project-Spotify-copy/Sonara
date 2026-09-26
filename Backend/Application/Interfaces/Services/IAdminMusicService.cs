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


    Task<TrackDto> UpdateTrackAsync(Guid id, UpdateTrackDto dto);
    Task DeleteTrackAsync(Guid id);
    Task<AlbumDto> UpdateAlbumAsync(Guid id, UpdateAlbumDto dto);
    Task DeleteAlbumAsync(Guid id);

}