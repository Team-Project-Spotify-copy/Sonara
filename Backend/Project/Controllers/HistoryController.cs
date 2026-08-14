using Application.DTOs.Music;
using Application.Helpers;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

/// <summary>
/// Історія прослуховувань поточного користувача. Записи створюються через
/// POST /api/tracks/{id}/listen і представляють відтворення, а не тіки прогресу.
/// </summary>
[ApiController]
[Route("api/history")]
[Authorize]
[Produces("application/json")]
public class HistoryController : ControllerBase
{
    private const int DefaultPageSize = 20;

    private readonly ITrackInteractionService _trackInteractionService;
    private readonly ICurrentUserService _currentUser;

    public HistoryController(ITrackInteractionService trackInteractionService, ICurrentUserService currentUser)
    {
        _trackInteractionService = trackInteractionService;
        _currentUser = currentUser;
    }

    /// <summary>Історія поточного користувача, найновіші прослуховування спочатку.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ListeningHistoryEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedList<ListeningHistoryEntryDto>>> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken ct = default)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");

        return Ok(await _trackInteractionService.GetHistoryAsync(userId, page, pageSize, ct));
    }
}
