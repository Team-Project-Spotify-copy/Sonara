using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Podcast;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PodcastsController : ControllerBase
    {
        private readonly IPodcastService _podcastService;

        private readonly ICurrentUserService _currentUser;

        public PodcastsController(IPodcastService podcastService, ICurrentUserService currentUser)
        {
            _podcastService = podcastService;
            _currentUser = currentUser;
        }

        // GET: api/podcasts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PodcastDto>>> GetAll()
        {
            var podcasts = await _podcastService.GetAllAsync();
            return Ok(podcasts);
        }

        [HttpGet("my")]
        [ProducesResponseType(typeof(IReadOnlyList<PodcastDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IReadOnlyList<PodcastDto>>> GetMy(CancellationToken ct)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");
            var podcasts = await _podcastService.GetMyPodcastsAsync(userId, ct);
            return Ok(podcasts);
        }

        // GET: api/podcasts/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PodcastDetailsDto>> GetById(Guid id)
        {
            var podcast = await _podcastService.GetByIdAsync(id);
            if (podcast == null) return NotFound();
            return Ok(podcast);
        }

        // POST: api/podcasts
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PodcastDto>> Create([FromForm] CreatePodcastDto dto)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");

            var createdPodcast = await _podcastService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = createdPodcast.Id }, createdPodcast);
        }

        // PUT: api/podcasts/{id}
        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<PodcastDto>> Update(Guid id, [FromForm] UpdatePodcastDto dto)
        {
            try
            {
                var userId = _currentUser.UserId
                    ?? throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");
                var updated = await _podcastService.UpdateAsync(id, userId, dto);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // DELETE: api/podcasts/{id}
        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = _currentUser.UserId
                    ?? throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");
                var success = await _podcastService.DeleteAsync(id, userId);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}