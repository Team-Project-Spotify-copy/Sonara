using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Commands.Auth;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;

namespace Backend.Project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRecaptchaServices _recaptchaServices;

    public AuthController(IMediator mediator, IRecaptchaServices recaptchaServices) {
        _mediator = mediator; 
        _recaptchaServices = recaptchaServices;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        bool isHuman = await _recaptchaServices.VerifyTokenAsync(command.token, "register_submit");

        if (!isHuman)
            return BadRequest("Invalid reCAPTCHA token.");

        var userId = await _mediator.Send(command, cancellationToken);

        return Ok(new { UserId = userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        bool isHuman = await _recaptchaServices.VerifyTokenAsync(command.token, "login_submit");

        if (!isHuman)
            return BadRequest("Invalid reCAPTCHA token.");

        var result = await _mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(new { AccessToken = result.AccessToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized();

        var result = await _mediator.Send(new RefreshCommand(refreshToken), cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(new { AccessToken = result.AccessToken });
    }

    private void SetRefreshTokenCookie(string token, DateTime expires)
    {
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expires
        });
    }
}
