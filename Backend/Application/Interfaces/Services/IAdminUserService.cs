namespace Application.Interfaces.Services;

using Application.DTOs.Users;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(string? search, CancellationToken ct = default);
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct = default);
    Task<AdminUserDto> UpdateUserRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
}