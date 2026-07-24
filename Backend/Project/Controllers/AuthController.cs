using Application.Entities.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;
namespace Backend.Project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly SonaraDbContext _context;

    public AuthController(ITokenService tokenService, SonaraDbContext context)
    {
        _tokenService = tokenService;
        _context = context;
    }

// для тесту дає рандом чела потім поміняємо ближче до релізу на норм логін
[HttpPost("login")]
public IActionResult Login(/* LoginDto loginDto */)
{
    var user = _context.Users.FirstOrDefault();

    if (user == null)
    {
        return BadRequest(new { Message = "У базі даних ще немає жодного користувача! Будь ласка, створіть хоча б одного." });
    }
    var accessToken = _tokenService.GenerateAccessToken(user);
    var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

    _context.RefreshTokens.Add(refreshToken);
    _context.SaveChanges();

    SetRefreshTokenCookie(refreshToken.Token, refreshToken.ExpiresAt);

    return Ok(new { AccessToken = accessToken });
}

    private void SetRefreshTokenCookie(string token, DateTime expires)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, 
            SameSite = SameSiteMode.Strict,
            Expires = expires
        };

        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}