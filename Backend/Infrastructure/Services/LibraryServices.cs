using Application.DTOs.Library;
using Application.Interfaces.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services;

public class LibraryServices : ILibraryServices
{
    private readonly SonaraDbContext _db;
    private readonly IPlaylistService _playlistService;
    private readonly IPodcastService _podcastService;
    private readonly IAlbumService _albumService;
    private readonly IMapper _mapper;
    public LibraryServices(SonaraDbContext db, IPlaylistService playlistService, IPodcastService podcastService, IAlbumService albumService, IMapper mapper)
    {
        _db = db;
        _playlistService = playlistService;
        _podcastService = podcastService;
        _albumService = albumService;
        _mapper = mapper;
    }

    public async Task<List<LibraryItemDto>> GetLibraryAsync(Guid userId, CancellationToken ct = default)
    {
        var playlists = await GetPlaylistsAsync(userId);
        var podcasts = await GetPodcastsAsync(userId);
        var albums = await GetAlbumsAsync(userId);
        var artists = await GetArtistsAsync(userId);

        return playlists
            .Concat(podcasts)
            .Concat(albums)
            .Concat(artists)
            .ToList();
    }

    public async Task<List<LibraryItemDto>> GetPlaylistsAsync(Guid userId, CancellationToken ct = default)
    {
        var playlists = await _playlistService.GetMyPlaylistsAsync(userId, ct);
        return _mapper.Map<List<LibraryItemDto>>(playlists);
    }

    public async Task<List<LibraryItemDto>> GetAlbumsAsync(Guid userId, CancellationToken ct = default)
    {
        var albums = await _albumService.GetMyAlbumsAsync(userId, ct);

        return _mapper.Map<List<LibraryItemDto>>(albums);
    }

    public async Task<List<LibraryItemDto>> GetPodcastsAsync(Guid userId, CancellationToken ct = default)
    {
        var podcasts = await _podcastService.GetMyPodcastsAsync(userId, ct);

        return _mapper.Map<List<LibraryItemDto>>(podcasts);
    }

    public async Task<List<LibraryItemDto>> GetArtistsAsync(Guid userId, CancellationToken ct = default)
    {
        var artists = await _db.Artists
            .AsNoTracking()
            .Where(a => a.User.Followers.Any(f => f.FollowerId == userId))
            .Include(a => a.User)
                .ThenInclude(u => u.Followers)
            .ToListAsync(ct);

        return _mapper.Map<List<LibraryItemDto>>(artists);
    }
}
