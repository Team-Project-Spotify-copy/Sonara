namespace Application.Interfaces.Services;

/// <summary>
/// Identity of the current request. The WebApp implementation reads it EXCLUSIVELY from
/// validated JWT claims - identifiers supplied by the client (headers, body, query) are never trusted.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Id of the authenticated user, or null for an anonymous request.</summary>
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}
