using Domain.Entities.Users;

namespace Application.Interfaces;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<User?> GetUserByEmailAsync(string email);
    Task<Guid> GetDefaultRoleIdAsync();
    Task<UserSubscription> CreateDefaultSubscriptionForUserAsync(Guid userId);
    Task AddUserAsync(User user);
    Task<RefreshToken?> GetActiveRefreshTokenAsync(string token);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task SaveChangesAsync();
    Task RevokeAllRefreshTokensAsync(Guid userId);
}
