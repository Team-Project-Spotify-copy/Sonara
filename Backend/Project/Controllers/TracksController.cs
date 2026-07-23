using Application.Interfaces;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using WebApp.Contracts;

namespace WebApp.Controllers;

[ApiController]
[Route("api/tracks")]
public class TracksController : ControllerBase
{
    private readonly ITrackInteractionService _trackInteractionService;
    private readonly ICurrentUserService _currentUser;

    public TracksController(ITrackInteractionService trackInteractionService, ICurrentUserService currentUser)
    {
        _trackInteractionService = trackInteractionService;
        _currentUser = currentUser;
    }

    [HttpPost("{id:guid}/like")]
    public async Task<IActionResult> Like(Guid id, CancellationToken ct)
    {
        var userId = RequireUserId();
        await _trackInteractionService.LikeAsync(id, userId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/like")]
    public async Task<IActionResult> Unlike(Guid id, CancellationToken ct)
    {
        var userId = RequireUserId();
        await _trackInteractionService.UnlikeAsync(id, userId, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/listen")]
    public async Task<IActionResult> RegisterListen(Guid id, [FromBody] RegisterListenRequest request, CancellationToken ct)
    {
        var userId = RequireUserId();
        await _trackInteractionService.RegisterListenAsync(id, userId, request.DurationListenedMs, ct);
        return NoContent();
    }

    private Guid RequireUserId()
    {
        return _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Не вказано X-User-Id (тимчасова заглушка автентифікації).");
    }
}