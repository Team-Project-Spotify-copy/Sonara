using Application.DTOs.Music;
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
    public async Task<ActionResult<Guid>> CreateTrack([FromBody] CreateTrackDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var trackId = await _adminMusicService.CreateTrackAsync(dto);

        // Немає GET-маршруту для одного треку, тому 201/Location тут не віддаємо.
        return Ok(trackId);
    }

    [HttpPost("albums")]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<Guid>> CreateAlbum([FromBody] CreateAlbumDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var albumId = await _adminMusicService.CreateAlbumAsync(dto);
        return CreatedAtAction(nameof(CatalogController.GetAlbum), "Catalog", new { id = albumId }, albumId);
    }
}