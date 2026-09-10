using Application.DTOs.Auth;
using Application.Exceptions;
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

    private const int MinimumPasswordLength = 8;

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        Validate(request);

        if (await _authRepository.EmailExistsAsync(request.Email))
            throw new ConflictException("Email is already registered.");

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = await _authRepository.GetDefaultRoleIdAsync(),
            ActiveSubscriptionId = null,
            ArtistProfile = new Artist
            {
                Id = Guid.NewGuid(),
                Name = request.Username,
            }
        };

        await _authRepository.AddUserAsync(user);
        await _authRepository.SaveChangesAsync();

        var freeSub = await _authRepository.CreateDefaultSubscriptionForUserAsync(user.Id);

        user.ActiveSubscriptionId = freeSub.Id;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        await _authRepository.AddRefreshTokenAsync(refreshToken);
        await _authRepository.SaveChangesAsync();

        return new AuthResultDto(user.Id, accessToken, refreshToken.Token, refreshToken.ExpiresAt);
    }

    private static void Validate(RegisterCommand request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            errors["email"] = new[] { "A valid email address is required." };

        if (string.IsNullOrWhiteSpace(request.Username))
            errors["username"] = new[] { "Username is required." };

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < MinimumPasswordLength)
            errors["password"] = new[] { $"Password must be at least {MinimumPasswordLength} characters long." };

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}