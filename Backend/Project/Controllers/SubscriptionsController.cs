using Application.DTOs.Subscription;
using Application.Interfaces.Services;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICurrentUserService _currentUser;

    public SubscriptionController(ISubscriptionService subscriptionService, ICurrentUserService currentUser)
    {
        _subscriptionService = subscriptionService;
        _currentUser = currentUser;
    }


    [HttpGet("plans")]
    public async Task<ActionResult<IReadOnlyList<SubscriptionPlanDto>>> GetAllPlans(CancellationToken ct)
    {
        return Ok(await _subscriptionService.GetAllPlansAsync(ct));
    }

    [HttpGet("plans/{id:guid}")]
    public async Task<ActionResult<SubscriptionPlanDto>> GetPlanById(Guid id, CancellationToken ct)
    {
        var plan = await _subscriptionService.GetPlanByIdAsync(id, ct);
        if (plan == null)
            return NotFound();

        return Ok(plan);
    }

    [HttpPost("plans")]
    public async Task<ActionResult<SubscriptionPlanDto>> CreatePlan(CreateSubscriptionPlanDto dto, CancellationToken ct)
    {
        var created = await _subscriptionService.CreatePlanAsync(dto, ct);
        return CreatedAtAction(nameof(GetPlanById), new { id = created.Id }, created);
    }

    [HttpPut("plans/{id:guid}")]
    public async Task<ActionResult<SubscriptionPlanDto>> UpdatePlan(Guid id, UpdateSubscriptionPlanDto dto, CancellationToken ct)
    {
        var updated = await _subscriptionService.UpdatePlanAsync(id, dto, ct);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("plans/{id:guid}")]
    public async Task<IActionResult> DeletePlan(Guid id, CancellationToken ct)
    {
        var result = await _subscriptionService.RemovePlanAsync(id, ct);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("subscription-status")]
    public async Task<IActionResult> GetSubscriptionStatus(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();

        var sub = await _subscriptionService.GetUserSubscriptionAsync(userId.Value, ct);
        var isActive = sub != null && sub.ExpiresAt > DateTime.UtcNow;

        return Ok(new { hasActiveSubscription = isActive });
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserSubscriptionDto>> GetMySubscription(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();

        var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId.Value, ct);
        if (subscription == null)
            return NotFound("У вас немає активної підписки.");

        return Ok(subscription);
    }

    [HttpPost("invite")]
    public async Task<IActionResult> InviteUser([FromBody] InviteUserRequestDto dto, CancellationToken ct)
    {
        var ownerId = _currentUser.UserId;

        if (ownerId == null)
            return Unauthorized();

        var result = await _subscriptionService.InviteToSubscriptionAsync(ownerId.Value, dto.TargetUsername, ct);

        if (!result)
            return BadRequest("Неможливо додати користувача: ліміт вичерпано або користувача не знайдено.");

        return Ok();
    }

    [HttpPost("remove-member/{userIdToRemove:guid}")]
    public async Task<IActionResult> RemoveMember(Guid userIdToRemove, [FromQuery] Guid activeSubId, CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        if (userId == null)
            return Unauthorized();

        var result = await _subscriptionService.LeaveOrRemoveFromSubscriptionAsync(userId.Value, activeSubId, userIdToRemove, ct);
        if (!result)
            return BadRequest("Помилка при видаленні користувача з підписки.");

        return Ok();
    }

    [HttpDelete("cancel/{activeSubId:guid}")]
    public async Task<IActionResult> CancelSubscription(Guid activeSubId, CancellationToken ct)
    {
        if (_currentUser.UserId == null)
            return Unauthorized();

        Guid currentUserId = _currentUser.UserId.Value;

        var result = await _subscriptionService.CancelSubscriptionAsync(currentUserId, activeSubId, ct);

        if (!result)
            return BadRequest("Не вдалося скасувати підписку. Перевірте, чи ви є власником цієї підписки.");

        return Ok();
    }
}