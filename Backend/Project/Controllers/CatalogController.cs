using Application.DTOs.Music;
using Application.Helpers;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Contracts;

namespace WebApp.Controllers;

/// <summary>
/// Читання каталогу. Доступне анонімно; для автентифікованих запитів у треках
/// додатково заповнюється поле isLiked.
/// </summary>
[ApiController]
[Route("api")]
[AllowAnonymous]
[Produces("application/json")]
public class CatalogController : ControllerBase
{
    private readonly IMusicCatalogService _catalogService;
    private readonly ICurrentUserService _currentUser;

    public CatalogController(IMusicCatalogService catalogService, ICurrentUserService currentUser)
    {
        _catalogService = catalogService;
        _currentUser = currentUser;
    }

    /// <summary>Сторінка каталогу треків із фільтрами та сортуванням.</summary>
    [HttpGet("tracks")]
    [ProducesResponseType(typeof(PaginatedList<TrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<TrackDto>>> GetTracks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? genre = null,
        [FromQuery] Guid? artistId = null,
        [FromQuery] Guid? albumId = null,
        [FromQuery] TrackSortOrder sort = TrackSortOrder.Newest,
        CancellationToken ct = default)
    {
        var result = await _catalogService.GetTracksAsync(
            new TrackQuery(page, pageSize, genre, artistId, albumId, sort),
            _currentUser.UserId,
            ct);

        return Ok(result);
    }

    /// <summary>Повні дані треку для сторінки треку та плеєра.</summary>
    [HttpGet("tracks/{id:guid}")]
    [ProducesResponseType(typeof(TrackDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackDetailsDto>> GetTrack(Guid id, CancellationToken ct)
    {
        return Ok(await _catalogService.GetTrackByIdAsync(id, _currentUser.UserId, ct));
    }

    /// <summary>
    /// Пакетне читання треків за id одним запитом - для відновлення черги плеєра
    /// без окремого звернення на кожен трек. Порядок відповіді збігається з порядком запиту.
    /// </summary>
    [HttpPost("tracks/batch")]
    [ProducesResponseType(typeof(IReadOnlyList<TrackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TrackDto>>> GetTracksBatch(
        [FromBody] TrackBatchRequest request,
        CancellationToken ct)
    {
        return Ok(await _catalogService.GetTracksByIdsAsync(request.Ids, _currentUser.UserId, ct));
    }

    [HttpGet("albums/{id:guid}")]
    [ProducesResponseType(typeof(AlbumDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlbumDto>> GetAlbum(Guid id, CancellationToken ct)
    {
        return Ok(await _catalogService.GetAlbumByIdAsync(id, _currentUser.UserId, ct));
    }

    [HttpGet("artists/{id:guid}")]
    [ProducesResponseType(typeof(ArtistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtistDto>> GetArtist(Guid id, CancellationToken ct)
    {
        return Ok(await _catalogService.GetArtistByIdAsync(id, _currentUser.UserId, ct));
    }

    /// <summary>
    /// Пошук по каталогу. Регістронезалежний. Порожній або надто короткий запит
    /// повертає 200 з порожніми секціями, а не помилку.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchResponseDto>> Search(
        [FromQuery] string? q,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        return Ok(await _catalogService.SearchAsync(q, limit, _currentUser.UserId, ct));
    }

    /// <summary>Популярні треки для стрічки головної сторінки (гілка main, кешується Redis).</summary>
    [HttpGet("tracks/popular")]
    [ProducesResponseType(typeof(IReadOnlyList<TrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TrackDto>>> GetPopularTracks(
        [FromQuery] int count = 20,
        CancellationToken ct = default)
    {
        return Ok(await _catalogService.GetPopularTracksAsync(count, _currentUser.UserId, ct));
    }

    /// <summary>Популярні альбоми для стрічки головної сторінки (гілка main, кешується Redis).</summary>
    [HttpGet("albums/popular")]
    [ProducesResponseType(typeof(IReadOnlyList<AlbumSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AlbumSummaryDto>>> GetPopularAlbums(
        [FromQuery] int count = 20,
        CancellationToken ct = default)
    {
        return Ok(await _catalogService.GetPopularAlbumsAsync(count, ct));
    }
}
