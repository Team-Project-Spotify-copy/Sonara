namespace Application.DTOs.Auth;

public record AuthResultDto(Guid? UserId, string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt);
