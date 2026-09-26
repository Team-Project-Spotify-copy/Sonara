namespace Infrastructure.Services;

using Application.DTOs.Music;
using Application.DTOs.Playlists;
using Application.DTOs.Users;
using Application.Enums;
using Application.Interfaces;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities.Social;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public class ProfileService : IProfileService
{
    private readonly SonaraDbContext _context;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;

    public ProfileService(SonaraDbContext context, IBlobService blobService, IMapper mapper)
    {
        _context = context;
        _blobService = blobService;
        _mapper = mapper;
    }

    public async Task<bool> FollowOnUser(Guid userId, string username, CancellationToken ct = default)
    {
        var targetUser = await _context.Users
            .Where(u => u.Username == username || (u.ArtistProfile != null && u.ArtistProfile.Name == username))
            .Select(u => new { u.Id, u.Username })
            .FirstOrDefaultAsync(ct);

        if (targetUser == null)
            throw new KeyNotFoundException($"User or artist with identifier '{username}' was not found.");

        if (targetUser.Id == userId)
            throw new InvalidOperationException("You cannot follow yourself.");

        var isAlreadyFollowing = await _context.Set<Follower>()
            .AnyAsync(f => f.FollowerId == userId && f.FollowedId == targetUser.Id, ct);

        if (isAlreadyFollowing)
            return false;

        var followerEntry = new Follower
        {
            FollowerId = userId,
            FollowedId = targetUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Follower>().Add(followerEntry);
        await _context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> UnFollowOnUser(Guid userId, string username, CancellationToken ct = default)
    {
        var followEntry = await _context.Followers
            .FirstOrDefaultAsync(f => f.FollowerId == userId &&
                (f.FollowedUser.Username == username || (f.FollowedUser.ArtistProfile != null && f.FollowedUser.ArtistProfile.Name == username)), ct);

        if (followEntry == null)
            return false;

        _context.Set<Follower>().Remove(followEntry);
        await _context.SaveChangesAsync(ct);

        return true;
    }
    public async Task<ProfileDto> GetUserByUsernameAsync(Guid userId, string username, CancellationToken ct = default)
    {
        var mapUser = await _context.Users
            .AsNoTracking()
            .Where(u => u.Username == username || (u.ArtistProfile != null && u.ArtistProfile.Name == username))
            .Select(u => new ProfileDto
            {
                Email = u.Email,
                Role = u.Role.Name,
                ArtistName = u.ArtistProfile != null ? u.ArtistProfile.Name : string.Empty,
                Username = u.Username,
                AvatarUrl = u.AvatarUrl,
                CreatedAt = u.CreatedAt,
                CountPlaylist = u.Playlists.Count,
                CountAlbum = u.ArtistProfile != null ? u.ArtistProfile.Albums.Count : 0,
                CountFollowers = u.Followers.Count,
                IsFollowing = u.Followers.Any(f => f.FollowerId == userId),

                Playlists = u.Playlists
                    .Where(p => !p.IsPrivate || p.UserId == userId)
                    .Select(p => new PlaylistDto(
                        p.Id,
                        p.UserId,
                        u.Username,
                        p.Name,
                        p.Description,
                        p.IsPrivate,
                        p.CoverUrl,
                        p.CreatedAt,
                        p.PlaylistTracks.Count,
                        p.PlaylistTracks.Sum(pt => pt.Track.DurationMs),
                        p.UserId == userId
                    )).ToList(),

                Albums = u.ArtistProfile != null
                    ? u.ArtistProfile.Albums.Select(album => new AlbumDto
                    {
                        Id = album.Id,
                        Title = album.Title,
                        CoverUrl = album.CoverUrl,
                        ReleaseDate = album.ReleaseDate,
                        Type = album.Type,
                        ArtistName = album.Artist != null ? album.Artist.Name : u.Username,
                        TracksCount = album.Tracks != null ? album.Tracks.Count : 0,
                        TotalDurationMs = album.Tracks != null ? album.Tracks.Sum(t => t.DurationMs) : 0
                    }).ToList()
                    : null,

                History = u.ListeningHistories
                    .OrderByDescending(h => h.ListenedAt)
                    .Select(h => new ListeningHistoryEntryDto
                    {
                        Id = h.Id,
                        ListenedAt = h.ListenedAt,
                        DurationListenedMs = h.DurationListenedMs,
                        Track = new TrackDto
                        {
                            Id = h.Track.Id,
                            Title = h.Track.Title,
                            ArtworkUrl = h.Track.Album != null ? h.Track.Album.CoverUrl : null
                        }
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct)
                ?? throw new KeyNotFoundException($"User or artist with identifier '{username}' was not found.");

        return mapUser;
    }
    public async Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var mapUser = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new ProfileDto
            {
                Email = u.Email,
                Role = u.Role.Name,
                ArtistName = u.ArtistProfile != null ? u.ArtistProfile.Name : string.Empty,
                Username = u.Username,
                AvatarUrl = u.AvatarUrl,
                CreatedAt = u.CreatedAt,
                CountPlaylist = u.Playlists.Count,
                CountFollowers = u.Followers.Count,
                IsFollowing = false,

                Playlists = u.Playlists
                    .Select(p => new PlaylistDto(
                        p.Id,
                        p.UserId,
                        u.Username,
                        p.Name,
                        p.Description,
                        p.IsPrivate,
                        p.CoverUrl,
                        p.CreatedAt,
                        p.PlaylistTracks.Count,
                        p.PlaylistTracks.Sum(pt => pt.Track.DurationMs),
                        true
                    )).ToList(),

                Albums = u.ArtistProfile != null
                    ? u.ArtistProfile.Albums.Select(album => new AlbumDto
                    {
                        Id = album.Id,
                        Title = album.Title,
                        CoverUrl = album.CoverUrl,
                        ReleaseDate = album.ReleaseDate,
                        Type = album.Type,
                        ArtistName = u.ArtistProfile.Name,
                        TracksCount = album.Tracks != null ? album.Tracks.Count : 0,
                        TotalDurationMs = album.Tracks != null ? album.Tracks.Sum(t => t.DurationMs) : 0
                    }).ToList()
                    : null,

                History = u.ListeningHistories
                    .OrderByDescending(h => h.ListenedAt)
                    .Select(h => new ListeningHistoryEntryDto
                    {
                        Id = h.Id,
                        ListenedAt = h.ListenedAt,
                        DurationListenedMs = h.DurationListenedMs,
                        Track = new TrackDto
                        {
                            Id = h.Track.Id,
                            Title = h.Track.Title,
                            ArtworkUrl = h.Track.Album != null ? h.Track.Album.CoverUrl : null
                        }
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct)
                ?? throw new KeyNotFoundException($"User with ID {userId} was not found.");

        return mapUser;
    }

    public async Task<bool> SetAvatarAsync(Guid userId, IFormFile avatarFile, CancellationToken ct = default)
    {
        await UpdateAvatarAsync(userId, avatarFile, ct);
        return true;
    }

    public async Task<string> UpdateAvatarAsync(Guid userId, IFormFile avatarFile, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.ArtistProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new KeyNotFoundException($"User with ID {userId} was not found.");

        string newUrl = await _blobService.ReplaceFileAsync(avatarFile, user.AvatarUrl, BlobFolder.Avatars);

        if (string.IsNullOrEmpty(newUrl))
            throw new InvalidOperationException("Failed to upload avatar");

        user.AvatarUrl = newUrl;

        if (user.ArtistProfile != null)
        {
            user.ArtistProfile.AvatarUrl = newUrl;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return newUrl;
    }

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto profileDto, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.ArtistProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new KeyNotFoundException($"User with ID {userId} was not found.");

        _mapper.Map(profileDto, user);

        if (profileDto.AvatarFile != null)
        {
            string newUrl = await _blobService.ReplaceFileAsync(profileDto.AvatarFile, user.AvatarUrl, BlobFolder.Avatars);

            if (string.IsNullOrEmpty(newUrl))
                throw new InvalidOperationException("Failed to upload avatar");

            user.AvatarUrl = newUrl;

            if (user.ArtistProfile != null)
            {
                user.ArtistProfile.AvatarUrl = newUrl;
            }
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return await GetProfileAsync(userId, ct);
    }
}