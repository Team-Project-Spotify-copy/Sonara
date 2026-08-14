using System.Security.Claims;
using Application.Interfaces.Services;

namespace WebApp.Services;

/// <summary>
/// Ідентичність поточного запиту береться ВИКЛЮЧНО з перевіреного JWT.
/// Ідентифікатори, надіслані клієнтом (заголовки, query, тіло), як ідентичність не використовуються.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true ? user : null;
        }
    }

    public Guid? UserId
    {
        get
        {
            var principal = Principal;
            if (principal is null)
            {
                return null;
            }

            // JwtBearer за замовчуванням мапить "sub" на NameIdentifier; перевіряємо обидва,
            // щоб сервіс продовжив працювати, якщо мапінг вхідних claims вимкнуть.
            var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? principal.FindFirstValue("sub");

            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => Principal is not null;

    public bool IsInRole(string role) => Principal?.IsInRole(role) == true;
}
