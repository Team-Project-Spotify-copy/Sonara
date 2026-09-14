namespace Application.DTOs.Music;

public enum RecommendationStrategy
{
    /// <summary>Ranked from the caller's own likes and listening history.</summary>
    Personalized = 0,

    /// <summary>The caller has no genre activity yet, so the most played tracks are returned.</summary>
    Popular = 1
}

public class RecommendationItemDto
{
    public TrackDto Track { get; set; } = new();

    /// <summary>
    /// Genre affinity accumulated from the caller's likes and plays. Higher is a
    /// stronger match; the value is comparable only inside one response, and is 0
    /// for popularity-only entries.
    /// </summary>
    public double Score { get; set; }

    /// <summary>The caller's own genres this track matched, strongest first.</summary>
    public List<string> MatchedGenres { get; set; } = new();
}

public class RecommendationsDto
{
    public RecommendationStrategy Strategy { get; set; }

    /// <summary>
    /// The genres that drove the ranking, strongest first. Empty when
    /// <see cref="Strategy"/> is <see cref="RecommendationStrategy.Popular"/>.
    /// </summary>
    public List<string> TopGenres { get; set; } = new();

    public List<RecommendationItemDto> Items { get; set; } = new();
}
