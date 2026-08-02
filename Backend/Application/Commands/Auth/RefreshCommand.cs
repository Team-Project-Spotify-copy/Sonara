using MediatR;
using Application.DTOs.Auth;
using Application.Interfaces;

namespace Application.Commands.Auth;

public record RefreshCommand(string RefreshToken) : IRequest<AuthResultDto>;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, AuthResultDto>
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;

    public RefreshCommandHandler(IAuthRepository authRepository, ITokenService tokenService)
    {
        _authRepository = authRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _authRepository.GetActiveRefreshTokenAsync(request.RefreshToken);

        if (existingToken is null || !existingToken.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        existingToken.RevokedAt = DateTime.UtcNow;

        var newAccessToken = _tokenService.GenerateAccessToken(existingToken.User);
        var newRefreshToken = _tokenService.GenerateRefreshToken(existingToken.UserId);

        await _authRepository.AddRefreshTokenAsync(newRefreshToken);
        await _authRepository.SaveChangesAsync();

        return new AuthResultDto(newAccessToken, newRefreshToken.Token, newRefreshToken.ExpiresAt);
    }
}
