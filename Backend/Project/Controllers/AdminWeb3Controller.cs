using Application.DTOs.Web3;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/admin/web3")]
[Authorize(Roles = "Admin,Moderator")]
public class AdminWeb3Controller : ControllerBase
{
    private readonly IAdminWeb3Service _adminWeb3Service;

    public AdminWeb3Controller(IAdminWeb3Service adminWeb3Service)
    {
        _adminWeb3Service = adminWeb3Service;
    }

    [HttpGet("logs")]
    [ProducesResponseType(typeof(IReadOnlyList<BlockchainLogDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<BlockchainLogDto>>> GetLogs(CancellationToken ct)
    {
        return Ok(await _adminWeb3Service.GetLogsAsync(ct));
    }

    [HttpPost("manual-activate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ManualActivate([FromBody] ManualActivateSubscriptionDto dto, CancellationToken ct)
    {
        try
        {
            await _adminWeb3Service.ManuallyActivateSubscriptionAsync(dto.UserId, dto.PlanType, ct);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}