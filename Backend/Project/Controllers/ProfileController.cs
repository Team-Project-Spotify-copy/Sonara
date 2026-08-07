using Application.DTOs.Users;
using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

public class ProfileController : ControllerBase
{
    private readonly IProfileServices _profileServices;

    public ProfileController(IProfileServices profileServices)
    {
        _profileServices = profileServices;
    }

    [HttpGet("api/profile")]
    public async Task<ActionResult<ProfileDto>> GetProfile(string username)
    {
        var userId = GetCurrentUserId();
        var profile = await _profileServices.GetProfileAsync(userId);
        return Ok(profile);
    }
    [HttpGet("api/profile/{username}")]
    public async Task<ActionResult<ProfileDto>> GetProfileByUsername(string username)
    {
        var profile = await _profileServices.GetUserByUsernameAsync(username);
        return Ok(profile);
    }

    [HttpPut("api/profile/update")]
    public async Task<ActionResult<ProfileDto>> UpdateProfile([FromBody] UpdateProfileDto profileDto)
    {
        var userId = GetCurrentUserId();
        var profile = await _profileServices.UpdateProfileAsync(userId, profileDto);
        return Ok(profile);
    }

    [HttpPost("api/profile/avatar")]
    public async Task<ActionResult<bool>> SetAvatar([FromForm] IFormFile avatarFile)
    {
        var userId = GetCurrentUserId();
        var result = await _profileServices.SetAvatarAsync(userId, avatarFile);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User ID claim not found in token");

        return Guid.Parse(userIdClaim);
    }
}
