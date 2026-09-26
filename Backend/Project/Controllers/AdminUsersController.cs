using Application.DTOs.Users;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin,Moderator")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdminUserDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetUsers([FromQuery] string? search, CancellationToken ct)
    {
        return Ok(await _adminUserService.GetUsersAsync(search, ct));
    }

    [HttpGet("roles")]
    [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetRoles(CancellationToken ct)
    {
        return Ok(await _adminUserService.GetRolesAsync(ct));
    }

    // Зміну ролі дозволяємо лише Admin — Moderator не повинен мати змогу
    // підвищити комусь (чи собі) права до Admin.
    [HttpPut("{id:guid}/role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AdminUserDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AdminUserDto>> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleDto dto, CancellationToken ct)
    {
        try
        {
            return Ok(await _adminUserService.UpdateUserRoleAsync(id, dto.RoleId, ct));
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
}