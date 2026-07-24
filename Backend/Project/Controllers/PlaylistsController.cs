using Application.DTOs.Playlists;
using Application.Interfaces;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using WebApp.Contracts;

namespace WebApp.Controllers;

[ApiController]
[Route("api/playlists")]
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
    public async Task<ActionResult<IReadOnlyList<PlaylistDto>>> GetMy(CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(await _playlistService.GetMyPlaylistsAsync(userId, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlaylistDto>> GetById(Guid id, CancellationToken ct)
    {
        var playlist = await _playlistService.GetByIdAsync(id, _currentUser.UserId, ct);
        return Ok(playlist);
    }

    [HttpPost]
    public async Task<ActionResult<PlaylistDto>> Create([FromBody] CreatePlaylistRequest request, CancellationToken ct)
    {
        var userId = RequireUserId();
        var created = await _playlistService.CreateAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlaylistDto>> Update(Guid id, [FromBody] UpdatePlaylistRequest request, CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(await _playlistService.UpdateAsync(id, userId, request, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = RequireUserId();
        await _playlistService.DeleteAsync(id, userId, ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/tracks")]
    public async Task<ActionResult<IReadOnlyList<PlaylistTrackDto>>> GetTracks(Guid id, CancellationToken ct)
    {
        return Ok(await _playlistService.GetTracksAsync(id, _currentUser.UserId, ct));
    }

    [HttpPost("{id:guid}/tracks")]
    public async Task<IActionResult> AddTrack(Guid id, [FromBody] AddTrackToPlaylistRequest request, CancellationToken ct)
    {
        var userId = RequireUserId();
        await _playlistService.AddTrackAsync(id, userId, request.TrackId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/tracks/{trackId:guid}")]
    public async Task<IActionResult> RemoveTrack(Guid id, Guid trackId, CancellationToken ct)
    {
        var userId = RequireUserId();
        await _playlistService.RemoveTrackAsync(id, userId, trackId, ct);
        return NoContent();
    }

    // TODO: замінити на [Authorize], коли буде JWT
    private Guid RequireUserId()
    {
        return _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Не вказано X-User-Id (тимчасова заглушка автентифікації).");
    }
}