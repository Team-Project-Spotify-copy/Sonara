using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Interfaces.Services;

namespace WebApp.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var idClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(idClaim, out var id) ? id : null;
        }
    }
}