using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities.Users;
using MediatR;

namespace Application.Commands.Auth;

public record RegisterCommand(string Email, string Username, string Password, string token) : IRequest<AuthResultDto>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(IAuthRepository authRepository, ITokenService tokenService)
    {
        _authRepository = authRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _authRepository.EmailExistsAsync(request.Email))
            throw new InvalidOperationException("Email is already registered.");

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = await _authRepository.GetDefaultRoleIdAsync(),
            ActiveSubscriptionId = null
        };

        await _authRepository.AddUserAsync(user);
        await _authRepository.SaveChangesAsync();

        var freeSub = await _authRepository.CreateDefaultSubscriptionForUserAsync(user.Id);

        user.ActiveSubscriptionId = freeSub.Id;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        await _authRepository.SaveChangesAsync();

        return new AuthResultDto(user.Id, accessToken, refreshToken.Token, refreshToken.ExpiresAt);
    }
}