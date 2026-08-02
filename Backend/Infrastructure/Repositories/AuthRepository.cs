using Microsoft.EntityFrameworkCore;
using Domain.Entities.Users;
using Application.Interfaces;

namespace Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly SonaraDbContext _context;

    public AuthRepository(SonaraDbContext context) => _context = context;

    public Task<bool> EmailExistsAsync(string email) =>
        _context.Users.AnyAsync(u => u.Email == email);

    public Task<User?> GetUserByEmailAsync(string email) =>
        _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);

    public async Task<Guid> GetDefaultRoleIdAsync() =>
        (await _context.Roles.FirstAsync(r => r.Name == "User")).Id;

    public async Task<Guid> GetDefaultSubscriptionIdAsync() =>
        (await _context.Subscriptions.FirstAsync(s => s.Name == "Free")).Id;

    public async Task AddUserAsync(User user) => await _context.Users.AddAsync(user);

    public Task<RefreshToken?> GetActiveRefreshTokenAsync(string token) =>
        _context.RefreshTokens
            .Include(rt => rt.User).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token);

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken) =>
        await _context.RefreshTokens.AddAsync(refreshToken);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
