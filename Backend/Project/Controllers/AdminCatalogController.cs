using Application.DTOs.Podcast;
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
    private readonly IAdminPodcastService _adminPodcastService;

    public AdminCatalogController(
        IAdminMusicService adminMusicService, 
        IAdminPodcastService adminPodcastService)
    {
        _adminMusicService = adminMusicService;
        _adminPodcastService = adminPodcastService;
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

    [HttpPut("tracks/{id:guid}")]
    [ProducesResponseType(typeof(TrackDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrackDto>> UpdateTrack(Guid id, [FromForm] UpdateTrackDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            return Ok(await _adminMusicService.UpdateTrackAsync(id, dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("tracks/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteTrack(Guid id)
    {
        try
        {
            await _adminMusicService.DeleteTrackAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("albums/{id:guid}")]
    [ProducesResponseType(typeof(AlbumDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AlbumDto>> UpdateAlbum(Guid id, [FromForm] UpdateAlbumDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            return Ok(await _adminMusicService.UpdateAlbumAsync(id, dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("albums/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAlbum(Guid id)
    {
        try
        {
            await _adminMusicService.DeleteAlbumAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("podcasts/{id:guid}")]
    [ProducesResponseType(typeof(PodcastDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PodcastDto>> UpdatePodcast(Guid id, [FromForm] UpdatePodcastDto dto)
    {
        try
        {
            return Ok(await _adminPodcastService.UpdatePodcastAsync(id, dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("podcasts/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeletePodcast(Guid id)
    {
        try
        {
            await _adminPodcastService.DeletePodcastAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
