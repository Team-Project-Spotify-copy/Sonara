using Application.Interfaces;

namespace WebApp.Services;

/// TODO: ТИМЧАСОВА реалізація, поки немає JWT. Бере UserId із заголовка "X-User-Id"

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
            var header = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();
            return Guid.TryParse(header, out var id) ? id : null;
        }
    }
}