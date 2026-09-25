using Application.DTOs.Auth;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities.Users;
using MediatR;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Application.Commands.Auth;

public record GoogleLoginCommand(string AccessToken) : IRequest<AuthResultDto>;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResultDto>
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;

    public GoogleLoginCommandHandler(
        IAuthRepository authRepository,
        ITokenService tokenService)
    {
        _authRepository = authRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", request.AccessToken);
        var googleResponse = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");

        if (!googleResponse.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException("Недійсний Google токен");
        }

        var userInfo = await googleResponse.Content.ReadFromJsonAsync<GoogleUserInfoDto>(cancellationToken: cancellationToken);

        if (userInfo?.Email is null)
        {
            throw new UnauthorizedAccessException("Не вдалося отримати email від Google");
        }

        var user = await _authRepository.GetUserByEmailAsync(userInfo.Email);

        if (user is null)
        {
            if (await _authRepository.EmailExistsAsync(userInfo.Email))
            {
                throw new ConflictException("Email is already registered.");
            }

            user = new User
            {
                Email = userInfo.Email,
                Username = userInfo.Name ?? userInfo.Email,
                PasswordHash = "EXTERNAL_AUTH_GOOGLE",
                RoleId = await _authRepository.GetDefaultRoleIdAsync(),
                ActiveSubscriptionId = null,
                ArtistProfile = new Artist
                {
                    Id = Guid.NewGuid(),
                    Name = userInfo.Name ?? userInfo.Email,
                }
            };

            await _authRepository.AddUserAsync(user);
            await _authRepository.SaveChangesAsync();

            var freeSub = await _authRepository.CreateDefaultSubscriptionForUserAsync(user.Id);
            user.ActiveSubscriptionId = freeSub.Id;

            await _authRepository.SaveChangesAsync();
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        await _authRepository.AddRefreshTokenAsync(refreshToken);
        await _authRepository.SaveChangesAsync();

        return new AuthResultDto(user.Id, accessToken, refreshToken.Token, refreshToken.ExpiresAt);
    }
}