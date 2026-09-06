using Application.Interfaces;
using MediatR;

namespace Application.Commands.Auth;

public record LogoutCommand(string? RefreshToken) : IRequest<bool>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IAuthRepository _authRepository;

    public LogoutCommandHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return false;
        }

        var existingToken = await _authRepository.GetActiveRefreshTokenAsync(request.RefreshToken);

        // Logging out is idempotent: an unknown or already revoked token is not an error,
        // the caller's cookie is cleared either way.
        if (existingToken is null || !existingToken.IsActive)
        {
            return false;
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        await _authRepository.SaveChangesAsync();

        return true;
    }
}
