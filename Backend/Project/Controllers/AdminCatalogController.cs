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

    /// <summary>
    /// Get-or-create an artist by name. Bulk imports are re-run over overlapping
    /// data, so this returns the existing row rather than duplicating it.
    /// </summary>
    [HttpPost("artists/resolve")]
    [ProducesResponseType(typeof(ResolvedEntityDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ResolvedEntityDto>> ResolveArtist([FromForm] ResolveArtistDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            return Ok(await _adminMusicService.ResolveArtistAsync(dto));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Get-or-create an album by artist + title.</summary>
    [HttpPost("albums/resolve")]
    [ProducesResponseType(typeof(ResolvedEntityDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ResolvedEntityDto>> ResolveAlbum([FromForm] ResolveAlbumDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            return Ok(await _adminMusicService.ResolveAlbumAsync(dto));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
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
