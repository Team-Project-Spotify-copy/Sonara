namespace Infrastructure.Services;

using Application.DTOs.Music;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Ranks tracks the user has not touched yet by how much their genres overlap the
/// genres the user already likes and plays. Likes and plays are the only inputs;
/// both already exist in the schema, so no extra bookkeeping is needed.
/// </summary>
public class RecommendationService : IRecommendationService
{
    public const int DefaultCount = 20;
    public const int MaxCount = 50;

    /// <summary>A like is deliberate; a play can be incidental, so likes pull harder.</summary>
    public const double LikeWeight = 3.0;
    public const double PlayWeight = 1.0;

    /// <summary>Only recent activity shapes the taste profile.</summary>
    public const int SeedLikes = 200;
    public const int SeedPlays = 200;

    /// <summary>Candidates are drawn from the strongest genres only, most played first.</summary>
    public const int TopGenresConsidered = 5;
    public const int CandidatePoolSize = 200;

    private readonly SonaraDbContext _db;

    public RecommendationService(SonaraDbContext db)
    {
        _db = db;
    }

    public async Task<RecommendationsDto> GetRecommendationsAsync(Guid userId, int count, CancellationToken ct = default)
    {
        var limit = Math.Clamp(count, 1, MaxCount);

        var likes = await _db.LikedTracks
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.LikedAt)
            .ThenBy(l => l.TrackId)
            .Take(SeedLikes)
            .Select(l => new SeedRow
            {
                TrackId = l.TrackId,
                ArtistId = l.Track.ArtistId,
                Genres = l.Track.TrackGenres
                    .Select(tg => new GenreRef { Id = tg.GenreId, Name = tg.Genre.Name })
                    .ToList()
            })
            .ToListAsync(ct);

        // Repeated plays are kept: playing something twice is the stronger signal.
        var plays = await _db.ListeningHistories
            .AsNoTracking()
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.ListenedAt)
            .ThenBy(h => h.Id)
            .Take(SeedPlays)
            .Select(h => new SeedRow
            {
                TrackId = h.TrackId,
                ArtistId = h.Track.ArtistId,
                Genres = h.Track.TrackGenres
                    .Select(tg => new GenreRef { Id = tg.GenreId, Name = tg.Genre.Name })
                    .ToList()
            })
            .ToListAsync(ct);

        var profile = BuildProfile(likes, plays);

        if (profile.GenreWeights.Count == 0)
        {
            // No genre to go on: fall back to what the whole catalog plays.
            var popular = await PopularExcludingAsync(userId, profile.SeenTrackIds, limit, ct);

            return new RecommendationsDto
            {
                Strategy = RecommendationStrategy.Popular,
                Items = popular.Select(track => new RecommendationItemDto { Track = track }).ToList()
            };
        }

        var topGenres = profile.GenreWeights
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => profile.GenreNames[pair.Key])
            .Take(TopGenresConsidered)
            .ToList();

        var ranked = await RankCandidatesAsync(profile, topGenres.Select(pair => pair.Key).ToList(), limit, ct);
        var pickedIds = ranked.Select(candidate => candidate.TrackId).ToList();

        // A narrow profile can leave the list short; popular unseen tracks fill it.
        var fillers = await PopularExcludingAsync(
            userId,
            profile.SeenTrackIds.Concat(pickedIds).ToHashSet(),
            limit - pickedIds.Count,
            ct);

        var tracksById = (await _db.Tracks
                .AsNoTracking()
                .Where(t => pickedIds.Contains(t.Id))
                .Select(CatalogProjections.Track(userId))
                .ToListAsync(ct))
            .ToDictionary(track => track.Id);

        return new RecommendationsDto
        {
            Strategy = RecommendationStrategy.Personalized,
            TopGenres = topGenres.Select(pair => profile.GenreNames[pair.Key]).ToList(),
            Items = ranked
                .Where(candidate => tracksById.ContainsKey(candidate.TrackId))
                .Select(candidate => new RecommendationItemDto
                {
                    Track = tracksById[candidate.TrackId],
                    Score = Math.Round(candidate.Score, 3),
                    MatchedGenres = candidate.MatchedGenres
                })
                .Concat(fillers.Select(track => new RecommendationItemDto { Track = track }))
                .ToList()
        };
    }

    private static TasteProfile BuildProfile(IReadOnlyList<SeedRow> likes, IReadOnlyList<SeedRow> plays)
    {
        var profile = new TasteProfile();

        foreach (var (rows, weight) in new[] { (likes, LikeWeight), (plays, PlayWeight) })
        {
            foreach (var row in rows)
            {
                profile.SeenTrackIds.Add(row.TrackId);

                foreach (var genre in row.Genres)
                {
                    profile.GenreWeights[genre.Id] = profile.GenreWeights.GetValueOrDefault(genre.Id) + weight;
                    profile.GenreNames[genre.Id] = genre.Name;
                }

                profile.ArtistWeights[row.ArtistId] = profile.ArtistWeights.GetValueOrDefault(row.ArtistId) + weight;
            }
        }

        return profile;
    }

    private async Task<List<RankedCandidate>> RankCandidatesAsync(
        TasteProfile profile,
        List<Guid> topGenreIds,
        int limit,
        CancellationToken ct)
    {
        var seenIds = profile.SeenTrackIds.ToList();

        var candidates = await _db.Tracks
            .AsNoTracking()
            .Where(t => !seenIds.Contains(t.Id) && t.TrackGenres.Any(tg => topGenreIds.Contains(tg.GenreId)))
            .OrderByDescending(t => t.PlaysCount)
            .ThenBy(t => t.Id)
            .Take(CandidatePoolSize)
            .Select(t => new CandidateRow
            {
                Id = t.Id,
                ArtistId = t.ArtistId,
                PlaysCount = t.PlaysCount,
                GenreIds = t.TrackGenres.Select(tg => tg.GenreId).ToList()
            })
            .ToListAsync(ct);

        return candidates
            .Select(candidate => Score(profile, candidate))
            .Where(candidate => candidate.Score > 0)
            .OrderByDescending(candidate => candidate.Score)
            // Equal genre pull: a familiar artist first, then plays, then a stable id.
            .ThenByDescending(candidate => candidate.ArtistAffinity)
            .ThenByDescending(candidate => candidate.PlaysCount)
            .ThenBy(candidate => candidate.TrackId)
            .Take(limit)
            .ToList();
    }

    private static RankedCandidate Score(TasteProfile profile, CandidateRow candidate)
    {
        var matched = new List<GenreRef>();
        var score = 0.0;

        // Scoring runs over the whole profile, not just the genres the pool was
        // filtered on, so matching several genres outranks one strong match.
        foreach (var genreId in candidate.GenreIds.Distinct())
        {
            if (!profile.GenreWeights.TryGetValue(genreId, out var weight))
            {
                continue;
            }

            score += weight;
            matched.Add(new GenreRef { Id = genreId, Name = profile.GenreNames[genreId] });
        }

        return new RankedCandidate
        {
            TrackId = candidate.Id,
            Score = score,
            ArtistAffinity = profile.ArtistWeights.GetValueOrDefault(candidate.ArtistId),
            PlaysCount = candidate.PlaysCount,
            MatchedGenres = matched
                .OrderByDescending(genre => profile.GenreWeights[genre.Id])
                .ThenBy(genre => genre.Name)
                .Select(genre => genre.Name)
                .ToList()
        };
    }

    private async Task<List<TrackDto>> PopularExcludingAsync(
        Guid userId,
        IReadOnlyCollection<Guid> excluded,
        int take,
        CancellationToken ct)
    {
        if (take <= 0)
        {
            return new List<TrackDto>();
        }

        var excludedIds = excluded.ToList();

        return await _db.Tracks
            .AsNoTracking()
            .Where(t => !excludedIds.Contains(t.Id))
            .OrderByDescending(t => t.PlaysCount)
            .ThenBy(t => t.Id)
            .Take(take)
            .Select(CatalogProjections.Track(userId))
            .ToListAsync(ct);
    }

    private sealed class TasteProfile
    {
        public Dictionary<Guid, double> GenreWeights { get; } = new();
        public Dictionary<Guid, string> GenreNames { get; } = new();
        public Dictionary<Guid, double> ArtistWeights { get; } = new();
        public HashSet<Guid> SeenTrackIds { get; } = new();
    }

    private sealed class GenreRef
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class SeedRow
    {
        public Guid TrackId { get; set; }
        public Guid ArtistId { get; set; }
        public List<GenreRef> Genres { get; set; } = new();
    }

    private sealed class CandidateRow
    {
        public Guid Id { get; set; }
        public Guid ArtistId { get; set; }
        public long PlaysCount { get; set; }
        public List<Guid> GenreIds { get; set; } = new();
    }

    private sealed class RankedCandidate
    {
        public Guid TrackId { get; set; }
        public double Score { get; set; }
        public double ArtistAffinity { get; set; }
        public long PlaysCount { get; set; }
        public List<string> MatchedGenres { get; set; } = new();
    }
}
