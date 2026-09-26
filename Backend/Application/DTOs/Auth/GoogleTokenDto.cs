namespace Application.DTOs.Auth;

public class GoogleTokenDto
{
    public string? AccessToken { get; set; } = null;
}

public class GoogleUserInfoDto
{
    public string? Email { get; set; } = null;
    public string? Name { get; set; } = null;
    public string? Sub { get; set; } = null;
}