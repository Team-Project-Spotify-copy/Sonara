namespace Application.Interfaces.Services;

using Application.DTOs.Music;
using Application.Helpers;

public interface IMusicCatalogService
{
    Task<PaginatedList<TrackDto>> GetTracksAsync(int pageNumber, int pageSize, string? genre);
    Task<AlbumDto> GetAlbumByIdAsync(Guid id);
    Task<ArtistDto> GetArtistByIdAsync(Guid id);
    Task<SearchResultDto> SearchAsync(string query, int limit = 10);
}