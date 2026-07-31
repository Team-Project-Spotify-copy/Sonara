using Application.DTOs.Subscription;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICurrentUserService _currentUser;

    public SubscriptionController(ISubscriptionService subscriptionService, ICurrentUserService currentUser)
    {
        _subscriptionService = subscriptionService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SubscriptionDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _subscriptionService.GetAllSubscriptionsAsync(ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubscriptionDto>> GetById(Guid id, CancellationToken ct)
    {
        var subscription = await _subscriptionService.GetSubscriptionsByIdAsync(id, ct);

        if (subscription == null)
            return NotFound();

        return Ok(subscription);
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionDto>> Create(CreateSubscriptionDto dto, CancellationToken ct)
    {
        var created = await _subscriptionService.CreateSubscriptionAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SubscriptionDto>> Update(Guid id, UpdateSubscriptionDto dto, CancellationToken ct)
    {
        var updated = await _subscriptionService.UpdateSubscriptionAsync(id, dto, ct);

        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _subscriptionService.RemoveSubscriptionAsync(id, ct);
        return NoContent();
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<SubscriptionDto>> GetUserSubscription(Guid userId, CancellationToken ct)
    {
        var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId, ct);

        if (subscription == null)
            return NotFound();

        return Ok(subscription);
    }

}