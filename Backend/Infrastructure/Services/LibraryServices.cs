using Application.DTOs.Library;
using Application.Interfaces.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services;

public class LibraryServices : ILibraryServices
{
    private readonly SonaraDbContext _db;
    private readonly IPlaylistService _playlistService;
    private readonly IMapper _mapper;
    public LibraryServices(SonaraDbContext db, IPlaylistService playlistService, IMapper mapper)
    {
        _db = db;
        _playlistService = playlistService;
        _mapper = mapper;
    }

    public async Task<List<LibraryItemDto>> GetLibraryAsync(Guid userId)
    {
        var playlists = await GetPlaylistsAsync(userId);
        var podcasts = await GetPodcastsAsync(userId);
        var artists = await GetArtistsAsync(userId);

        return playlists
            .Concat(podcasts)
            .Concat(artists)
            .ToList();
    }

    public async Task<List<LibraryItemDto>> GetPlaylistsAsync(Guid userId)
    {
        var playlists = await _playlistService.GetMyPlaylistsAsync(userId);
        return _mapper.Map<List<LibraryItemDto>>(playlists);
    }

    public async Task<List<LibraryItemDto>> GetPodcastsAsync(Guid userId)
    {
        var podcasts = await _db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Podcasts)
            .ToListAsync();

        return _mapper.Map<List<LibraryItemDto>>(podcasts);
    }

    public async Task<List<LibraryItemDto>> GetArtistsAsync(Guid userId)
    {
        var artists = await _db.Artists
            .Where(a => a.User.Followers.Any(f => f.FollowerId == userId))
            .Include(a => a.User)
                .ThenInclude(u => u.Followers)
            .ToListAsync();

        return _mapper.Map<List<LibraryItemDto>>(artists);
    }
}
