using MediatR;
using Application.DTOs.Auth;
using Application.Interfaces;

namespace Application.Commands.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResultDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IAuthRepository authRepository, ITokenService tokenService)
    {
        _authRepository = authRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        await _authRepository.AddRefreshTokenAsync(refreshToken);
        await _authRepository.SaveChangesAsync();

        return new AuthResultDto(accessToken, refreshToken.Token, refreshToken.ExpiresAt);
    }
}
