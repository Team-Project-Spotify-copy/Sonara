using Application.DTOs.Music;
using Application.Helpers;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Contracts;

namespace WebApp.Controllers;

[ApiController]
[Route("api/tracks")]
[Authorize]
[Produces("application/json")]
public class TracksController : ControllerBase
{
    private const int DefaultPageSize = 20;

    private readonly ITrackInteractionService _trackInteractionService;
    private readonly ITrackStreamService _trackStreamService;
    private readonly ICurrentUserService _currentUser;

    public TracksController(
        ITrackInteractionService trackInteractionService,
        ITrackStreamService trackStreamService,
        ICurrentUserService currentUser)
    {
        _trackInteractionService = trackInteractionService;
        _trackStreamService = trackStreamService;
        _currentUser = currentUser;
    }

    [HttpGet("{id:guid}/stream")]
    [ProducesResponseType(typeof(TrackStreamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<TrackStreamDto>> GetStream(Guid id, CancellationToken ct)
    {
        var stream = await _trackStreamService.ResolveAsync(id, ct);

        Response.Headers.CacheControl = "private, no-store";

        return Ok(stream);
    }

    [HttpGet("liked")]
    [ProducesResponseType(typeof(PaginatedList<TrackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedList<TrackDto>>> GetLiked(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken ct = default)
    {
        var userId = RequireUserId();
        return Ok(await _trackInteractionService.GetLikedTracksAsync(userId, page, pageSize, ct));
    }

    [HttpGet("{id:guid}/like")]
    [ProducesResponseType(typeof(TrackLikeStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackLikeStateDto>> GetLikeState(Guid id, CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(await _trackInteractionService.GetLikeStateAsync(id, userId, ct));
    }

    [HttpPost("{id:guid}/like")]
    [ProducesResponseType(typeof(TrackLikeStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackLikeStateDto>> Like(Guid id, CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(await _trackInteractionService.LikeAsync(id, userId, ct));
    }

    [HttpDelete("{id:guid}/like")]
    [ProducesResponseType(typeof(TrackLikeStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackLikeStateDto>> Unlike(Guid id, CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(await _trackInteractionService.UnlikeAsync(id, userId, ct));
    }

    [HttpPost("{id:guid}/listen")]
    [ProducesResponseType(typeof(ListenRegistrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListenRegistrationDto>> RegisterListen(
        Guid id,
        [FromBody] RegisterListenRequest request,
        CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(await _trackInteractionService.RegisterListenAsync(id, userId, request.DurationListenedMs, ct));
    }

    private Guid RequireUserId()
    {
        return _currentUser.UserId
            ?? throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");
    }
}
