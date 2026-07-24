using Application.Entities.Users;

namespace Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId);
}