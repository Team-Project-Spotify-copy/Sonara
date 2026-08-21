using Application.DTOs.Playlists;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Contracts;

namespace WebApp.Controllers;

[ApiController]
[Route("api/playlists")]
[Authorize]
[Produces("application/json")]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistService _playlistService;
    private readonly ICurrentUserService _currentUser;

    public PlaylistsController(IPlaylistService playlistService, ICurrentUserService currentUser)
    {
        _playlistService = playlistService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlaylistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<PlaylistDto>>> GetMy(CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(await _playlistService.GetMyPlaylistsAsync(userId, ct));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlaylistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaylistDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _playlistService.GetByIdAsync(id, _currentUser.UserId, ct));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PlaylistDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PlaylistDto>> Create([FromForm] CreatePlaylistRequest request, CancellationToken ct)
    {
        var userId = RequireUserId();
        var created = await _playlistService.CreateAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PlaylistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaylistDto>> Update(Guid id, [FromForm] UpdatePlaylistRequest request, CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(await _playlistService.UpdateAsync(id, userId, request, ct));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = RequireUserId();
        await _playlistService.DeleteAsync(id, userId, ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/tracks")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<PlaylistTrackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PlaylistTrackDto>>> GetTracks(Guid id, CancellationToken ct)
    {
        return Ok(await _playlistService.GetTracksAsync(id, _currentUser.UserId, ct));
    }

    [HttpPost("{id:guid}/tracks")]
    [ProducesResponseType(typeof(PlaylistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaylistDto>> AddTrack(Guid id, [FromBody] AddTrackToPlaylistRequest request, CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(await _playlistService.AddTrackAsync(id, userId, request.TrackId, ct));
    }

    [HttpDelete("{id:guid}/tracks/{trackId:guid}")]
    [ProducesResponseType(typeof(PlaylistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaylistDto>> RemoveTrack(Guid id, Guid trackId, CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(await _playlistService.RemoveTrackAsync(id, userId, trackId, ct));
    }

    private Guid RequireUserId()
    {
        return _currentUser.UserId
            ?? throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");
    }
}
