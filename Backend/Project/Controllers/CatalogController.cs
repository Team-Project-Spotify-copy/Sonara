using Application.DTOs.Music;
using Application.Helpers;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api")]
[Authorize] 
public class CatalogController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly IMusicCatalogService _catalogService;

    public CatalogController(IMusicCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("tracks")]
    [ProducesResponseType(typeof(PaginatedList<TrackDto>), 200)]
    public async Task<ActionResult<PaginatedList<TrackDto>>> GetTracks(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] string? genre = null)
    {
        var result = await _catalogService.GetTracksAsync(
            Math.Max(1, page),
            Math.Clamp(pageSize, 1, MaxPageSize),
            genre);
        return Ok(result);
    }

    [HttpGet("albums/{id:guid}")]
    [ProducesResponseType(typeof(AlbumDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AlbumDto>> GetAlbum(Guid id)
    {
        var result = await _catalogService.GetAlbumByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("artists/{id:guid}")]
    [ProducesResponseType(typeof(ArtistDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ArtistDto>> GetArtist(Guid id)
    {
        var result = await _catalogService.GetArtistByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchResultDto), 200)]
    public async Task<ActionResult<SearchResultDto>> Search([FromQuery] string q, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search query cannot be empty.");

        var result = await _catalogService.SearchAsync(q, limit);
        return Ok(result);
    }

    [HttpGet("tracks/popular")]
    [ProducesResponseType(typeof(List<TrackDto>), 200)]
    public async Task<ActionResult<List<TrackDto>>> GetPopularTracks([FromQuery] int count = 20)
    {
        var result = await _catalogService.GetPopularTracksAsync(Math.Clamp(count, 1, MaxPageSize));
        return Ok(result);
    }

    [HttpGet("albums/popular")]
    [ProducesResponseType(typeof(List<AlbumDto>), 200)]
    public async Task<ActionResult<List<AlbumDto>>> GetPopularAlbums([FromQuery] int count = 20)
    {
        var result = await _catalogService.GetPopularAlbumsAsync(Math.Clamp(count, 1, MaxPageSize));
        return Ok(result);
    }
}