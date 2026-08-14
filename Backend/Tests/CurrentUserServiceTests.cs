using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebApp.Services;
using Xunit;

namespace Sonara.Tests;

public class CurrentUserServiceTests
{
    private static CurrentUserService Build(HttpContext? context)
    {
        var accessor = new HttpContextAccessor { HttpContext = context };
        return new CurrentUserService(accessor);
    }

    private static HttpContext Authenticated(string claimType, string value, params string[] roles)
    {
        var claims = new List<Claim> { new(claimType, value) };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, authenticationType: "TestBearer", ClaimTypes.Name, ClaimTypes.Role);
        return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
    }

    [Fact]
    public void Anonymous_request_has_no_user()
    {
        var service = Build(new DefaultHttpContext());

        Assert.Null(service.UserId);
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public void No_http_context_has_no_user()
    {
        var service = Build(null);

        Assert.Null(service.UserId);
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public void Identity_comes_from_the_name_identifier_claim()
    {
        var userId = Guid.NewGuid();
        var service = Build(Authenticated(ClaimTypes.NameIdentifier, userId.ToString()));

        Assert.Equal(userId, service.UserId);
        Assert.True(service.IsAuthenticated);
    }

    [Fact]
    public void Identity_falls_back_to_the_sub_claim_when_inbound_mapping_is_off()
    {
        var userId = Guid.NewGuid();
        var service = Build(Authenticated("sub", userId.ToString()));

        Assert.Equal(userId, service.UserId);
    }

    [Fact]
    public void A_malformed_subject_claim_yields_no_user()
    {
        var service = Build(Authenticated(ClaimTypes.NameIdentifier, "not-a-guid"));

        Assert.Null(service.UserId);
    }

    /// <summary>
    /// Регресія: раніше ідентичність бралася із заголовка X-User-Id, тож будь-хто міг
    /// видати себе за іншого користувача. Тепер заголовки клієнта ігноруються повністю.
    /// </summary>
    [Fact]
    public void A_client_supplied_header_is_never_treated_as_identity()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = Guid.NewGuid().ToString();

        var service = Build(context);

        Assert.Null(service.UserId);
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public void A_client_supplied_header_cannot_override_the_token_subject()
    {
        var tokenUserId = Guid.NewGuid();
        var context = Authenticated(ClaimTypes.NameIdentifier, tokenUserId.ToString());
        context.Request.Headers["X-User-Id"] = Guid.NewGuid().ToString();

        var service = Build(context);

        Assert.Equal(tokenUserId, service.UserId);
    }

    [Fact]
    public void Roles_are_read_from_the_token()
    {
        var service = Build(Authenticated(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString(), "Admin"));

        Assert.True(service.IsInRole("Admin"));
        Assert.False(service.IsInRole("Moderator"));
    }

    [Fact]
    public void Roles_are_not_granted_to_anonymous_requests()
    {
        var service = Build(new DefaultHttpContext());

        Assert.False(service.IsInRole("Admin"));
    }
}
