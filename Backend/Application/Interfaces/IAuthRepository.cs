using Domain.Entities.Users;

namespace Application.Interfaces;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<User?> GetUserByEmailAsync(string email);
    Task<Guid> GetDefaultRoleIdAsync();
    Task<Guid> GetDefaultSubscriptionIdAsync();
    Task AddUserAsync(User user);
    Task<RefreshToken?> GetActiveRefreshTokenAsync(string token);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task SaveChangesAsync();
}
