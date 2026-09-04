using Application.DTOs.Users;
using Application.Interfaces.Services;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileServices;

    public ProfileController(IProfileService profileServices)
    {
        _profileServices = profileServices;
    }

    [HttpPost("api/profile/{username}/follow")]
    public async Task<ActionResult<bool>> FollowUser(string username)
    {
        var userId = GetCurrentUserId();
        var result = await _profileServices.FollowOnUser(userId, username);
        return Ok(result);
    }

    [HttpDelete("api/profile/{username}/unfollow")]
    public async Task<ActionResult<bool>> UnfollowUser(string username)
    {
        var userId = GetCurrentUserId();
        var result = await _profileServices.UnFollowOnUser(userId, username);
        return Ok(result);
    }

    [HttpGet("api/profile")]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        var userId = GetCurrentUserId();
        var profile = await _profileServices.GetProfileAsync(userId);
        return Ok(profile);
    }
    [AllowAnonymous]
    [HttpGet("api/profile/{username}")]
    public async Task<ActionResult<ProfileDto>> GetProfileByUsername(string username)
    {
        var userId = GetCurrentUserId();
        var profile = await _profileServices.GetUserByUsernameAsync(userId, username);
        return Ok(profile);
    }

    [HttpPut("api/profile/update")]
    public async Task<ActionResult<ProfileDto>> UpdateProfile([FromForm] UpdateProfileDto profileDto)
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
