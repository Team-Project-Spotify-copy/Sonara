using Application.DTOs.Music;
using Application.Interfaces;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,Moderator")]
public class AdminCatalogController : ControllerBase
{
    private readonly IAdminMusicService _adminMusicService;

    public AdminCatalogController(IAdminMusicService adminMusicService)
    {
        _adminMusicService = adminMusicService;
    }

    [HttpPost("tracks")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<Guid>> CreateTrack([FromForm] CreateTrackDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var trackId = await _adminMusicService.CreateTrackAsync(dto);

        return Ok(trackId);
    }

    [HttpPost("albums")]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<Guid>> CreateAlbum([FromForm] CreateAlbumDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var albumId = await _adminMusicService.CreateAlbumAsync(dto);
        return CreatedAtAction(nameof(CatalogController.GetAlbum), "Catalog", new { id = albumId }, albumId);
    }

    [HttpPost("subscription-reminders/trigger")]
    public async Task<IActionResult> TriggerSubscriptionReminders(
    [FromServices] ISubscriptionReminderService reminderService,
    CancellationToken ct)
    {
        var count = await reminderService.SendWeeklyRemindersAsync(force: true, ct);
        return Ok(new { SentCount = count });
    }
}
