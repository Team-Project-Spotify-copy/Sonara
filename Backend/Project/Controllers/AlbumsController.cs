using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.Music;
using Application.Interfaces.Services;

[ApiController]
[Route("api/albums")]
public class AlbumsController : ControllerBase
{
    private readonly IAlbumService _albumService;

    public AlbumsController(IAlbumService albumService)
    {
        _albumService = albumService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AlbumDto>>> GetAll(CancellationToken ct)
    {
        var albums = await _albumService.GetAllAsync(ct);
        return Ok(albums);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AlbumSummaryDto>> GetById(Guid id, CancellationToken ct)
    {
        var album = await _albumService.GetByIdAsync(id, ct);
        if (album == null) return NotFound();

        return Ok(album);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AlbumDto>> Create([FromForm] CreateAlbumDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var createdAlbum = await _albumService.CreateAsync(dto, userId, ct);

        return CreatedAtAction(nameof(GetById), new { id = createdAlbum.Id }, createdAlbum);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AlbumDto>> Update(Guid id, [FromForm] UpdateAlbumDto dto, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var updatedAlbum = await _albumService.UpdateAsync(id, dto, userId, ct);
            if (updatedAlbum == null) return NotFound();

            return Ok(updatedAlbum);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _albumService.DeleteAsync(id, userId, ct);
            if (!result) return NotFound();

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("{albumId:guid}/tracks")]
    public async Task<ActionResult<AlbumDto>> AddTrackToAlbum(Guid albumId, [FromQuery] string trackName, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var updatedAlbum = await _albumService.AddTrackToAlbumAsync(albumId, trackName, userId, ct);

            if (updatedAlbum == null)
                return NotFound(new { message = "Album or track not found." });

            return Ok(updatedAlbum);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }
}