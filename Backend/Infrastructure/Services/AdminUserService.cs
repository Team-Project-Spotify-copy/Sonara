namespace Infrastructure.Services;

using Application.DTOs.Users;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

public class AdminUserService : IAdminUserService
{
    private readonly SonaraDbContext _db;

    public AdminUserService(SonaraDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(string? search, CancellationToken ct = default)
    {
        var query = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                u.Username.ToLower().Contains(term));
        }

        return await query
            .OrderBy(u => u.Username)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Email,
                u.Username,
                u.AvatarUrl,
                u.RoleId,
                u.Role.Name,
                u.ActiveSubscription != null ? u.ActiveSubscription.Plan.Name : null,
                u.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct = default)
    {
        return await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(r.Id, r.Name))
            .ToListAsync(ct);
    }

    public async Task<AdminUserDto> UpdateUserRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new KeyNotFoundException($"User with ID {userId} was not found.");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId, ct)
            ?? throw new ArgumentException("Unknown role.");

        user.RoleId = roleId;
        await _db.SaveChangesAsync(ct);

        var subscriptionPlanName = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.ActiveSubscription != null ? u.ActiveSubscription.Plan.Name : null)
            .FirstAsync(ct);

        return new AdminUserDto(
            user.Id,
            user.Email,
            user.Username,
            user.AvatarUrl,
            role.Id,
            role.Name,
            subscriptionPlanName,
            user.CreatedAt);
    }
}