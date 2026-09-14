using Application.DTOs.Music;

namespace Application.Interfaces.Services;

public interface IRecommendationService
{
    /// <summary>
    /// Tracks the user has not liked or played yet, ranked by how well their genres
    /// overlap the genres the user already likes and listens to.
    /// </summary>
    Task<RecommendationsDto> GetRecommendationsAsync(Guid userId, int count, CancellationToken ct = default);
}
