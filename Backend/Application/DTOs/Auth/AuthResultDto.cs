namespace Application.DTOs.Auth;

public record AuthResultDto(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt);
