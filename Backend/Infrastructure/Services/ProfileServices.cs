using Application.DTOs.Music;
using Application.DTOs.Playlists;
using Application.DTOs.Users;
using Application.Enums;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Social;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ProfileServices : IProfileServices
{
    private readonly SonaraDbContext _context;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;

    public ProfileServices(SonaraDbContext context, IBlobService blobService, IMapper mapper)
    {
        _context = context;
        _blobService = blobService;
        _mapper = mapper;
    }

    public async Task<bool> FollowOnUser(Guid userId, string username)
    {
        var targetUser = await _context.Users
            .Select(u => new { u.Id, u.Username })
            .FirstOrDefaultAsync(u => u.Username == username);

        if (targetUser == null)
            throw new KeyNotFoundException($"User with username '{username}' was not found.");

        if (targetUser.Id == userId)
            throw new InvalidOperationException("You cannot follow yourself.");

        var isAlreadyFollowing = await _context.Set<Follower>()
            .AnyAsync(f => f.FollowerId == userId && f.FollowedId == targetUser.Id);

        if (isAlreadyFollowing)
            return false; 

        var followerEntry = new Follower
        {
            FollowerId = userId,
            FollowedId = targetUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Follower>().Add(followerEntry);
        await _context.SaveChangesAsync();

        return true; 
    }

    public async Task<bool> UnFollowOnUser(Guid userId, string username)
    {
        var followEntry = await _context.Followers
            .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowedUser.Username == username);
        
        if (followEntry == null)
            return false;

        _context.Set<Follower>().Remove(followEntry);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ProfileDto> GetProfileAsync(Guid userId)
    {
        var mapUser = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .ProjectTo<ProfileDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"User with ID {userId} was not found.");

        return mapUser;
    }

    public async Task<ProfileDto> GetUserByUsernameAsync(Guid userId, string username)
    {
        var mapUser = await _context.Users
            .AsNoTracking()
            .Where(u => u.Username == username)
            .Select(u => new ProfileDto
            {
                Email = u.Email,
                Username = u.Username,
                AvatarUrl = u.AvatarUrl,
                CreatedAt = u.CreatedAt,
                CountPlaylist = u.Playlists.Count,
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
            .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"User with username '{username}' was not found.");

        return mapUser;
    }

    public async Task<bool> SetAvatarAsync(Guid userId, IFormFile avatarFile)
    {
        await UpdateAvatarAsync(userId, avatarFile);
        return true;
    }


    public async Task<string> UpdateAvatarAsync(Guid userId, IFormFile avatarFile)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException($"User with ID {userId} was not found.");

        string newUrl = await _blobService.ReplaceFileAsync(avatarFile, user.AvatarUrl, BlobFolder.Avatars);

        if (string.IsNullOrEmpty(newUrl))
            throw new InvalidOperationException("Failed to upload avatar");

        user.AvatarUrl = newUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return newUrl;
    }

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto profileDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException($"User with ID {userId} was not found.");

        _mapper.Map(profileDto, user);

        if (profileDto.AvatarFile != null)
        {
            string newUrl = await _blobService.ReplaceFileAsync(profileDto.AvatarFile, user.AvatarUrl, BlobFolder.Avatars);

            if (string.IsNullOrEmpty(newUrl))
                throw new InvalidOperationException("Failed to upload avatar");

            user.AvatarUrl = newUrl;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetProfileAsync(userId);
    }
}