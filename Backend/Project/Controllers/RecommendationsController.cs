using Application.DTOs.Music;
using Application.Interfaces.Services;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/recommendations")]
[Authorize]
[Produces("application/json")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendations;
    private readonly ICurrentUserService _currentUser;

    public RecommendationsController(IRecommendationService recommendations, ICurrentUserService currentUser)
    {
        _recommendations = recommendations;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(RecommendationsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RecommendationsDto>> GetRecommendations(
        [FromQuery] int count = RecommendationService.DefaultCount,
        CancellationToken ct = default)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");

        return Ok(await _recommendations.GetRecommendationsAsync(userId, count, ct));
    }
}
