using Application.DTOs.Library;
using Application.DTOs.Music;
using Application.DTOs.Playlists;
using Application.DTOs.Podcast;
using Application.DTOs.Subscription;
using Application.DTOs.Users;
using AutoMapper;
using Domain.Entities.Music;
using Domain.Entities.Playlists;
using Domain.Entities.Podcasts;
using Domain.Entities.Users;

namespace Application.Configurations
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // 1. Track -> TrackDto
            CreateMap<Track, TrackDto>()
                .ForMember(dest => dest.ArtworkUrl, opt => opt.MapFrom(src =>
                    src.Album != null && src.Album.CoverUrl != null
                        ? src.Album.CoverUrl
                        : (src.Artist != null ? src.Artist.AvatarUrl : null)))
                .ForMember(dest => dest.IsLiked, opt => opt.Ignore());

            // 2. Track -> TrackDetailsDto
            CreateMap<Track, TrackDetailsDto>()
                .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src => src.Artist != null ? src.Artist.Name : string.Empty))
                .ForMember(dest => dest.ArtistAvatarUrl, opt => opt.MapFrom(src => src.Artist != null ? src.Artist.AvatarUrl : null))
                .ForMember(dest => dest.ArtistVerified, opt => opt.MapFrom(src => src.Artist != null && src.Artist.Verified))
                .ForMember(dest => dest.AlbumTitle, opt => opt.MapFrom(src => src.Album != null ? src.Album.Title : null))
                .ForMember(dest => dest.AlbumCoverUrl, opt => opt.MapFrom(src => src.Album != null ? src.Album.CoverUrl : null))
                .ForMember(dest => dest.AlbumType, opt => opt.MapFrom(src => src.Album != null ? src.Album.Type : null))
                .ForMember(dest => dest.AlbumReleaseDate, opt => opt.MapFrom(src => src.Album != null ? src.Album.ReleaseDate : null))
                .ForMember(dest => dest.ArtworkUrl, opt => opt.MapFrom(src =>
                    src.Album != null && src.Album.CoverUrl != null
                        ? src.Album.CoverUrl
                        : (src.Artist != null ? src.Artist.AvatarUrl : null)))
                .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.TrackGenres.Select(tg => tg.Genre.Name).ToList()))
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.LikedByUsers.Count))
                .ForMember(dest => dest.HasStream, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.AudioUrl)))
                .ForMember(dest => dest.IsLiked, opt => opt.Ignore());

            // Album
            CreateMap<Album, AlbumDto>()
                .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src => src.Artist != null ? src.Artist.Name : string.Empty))
                .ForMember(dest => dest.TracksCount, opt => opt.MapFrom(src => src.Tracks != null ? src.Tracks.Count : 0))
                .ForMember(dest => dest.TotalDurationMs, opt => opt.MapFrom(src => src.Tracks != null ? src.Tracks.Sum(t => t.DurationMs) : 0));

            CreateMap<Album, AlbumSummaryDto>()
                .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src => src.Artist != null ? src.Artist.Name : string.Empty))
                .ForMember(dest => dest.TracksCount, opt => opt.MapFrom(src => src.Tracks != null ? src.Tracks.Count : 0));

            // Commands / Resolves
            CreateMap<CreateTrackDto, Track>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title.Trim()))
                .ForMember(dest => dest.DurationMs, opt => opt.MapFrom(src => (int)Math.Round(src.DurationSeconds * 1000)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.AudioUrl, opt => opt.Ignore())
                .ForMember(dest => dest.TrackGenres, opt => opt.Ignore());

            CreateMap<ResolveArtistDto, Artist>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio != null ? src.Bio.Trim() : null))
                .ForMember(dest => dest.Verified, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.AvatarUrl, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<ResolveAlbumDto, Album>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title.Trim()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => src.ReleaseDate.HasValue
                    ? DateTime.SpecifyKind(src.ReleaseDate.Value, DateTimeKind.Utc)
                    : (DateTime?)null))
                .ForMember(dest => dest.CoverUrl, opt => opt.Ignore());

            CreateMap<CreateAlbumDto, Album>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title.Trim()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => DateTime.SpecifyKind(src.ReleaseDate, DateTimeKind.Utc)))
                .ForMember(dest => dest.CoverUrl, opt => opt.Ignore());

            // Subscription & User Short
            CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>();
            CreateMap<UpdateSubscriptionPlanDto, SubscriptionPlan>();
            CreateMap<SubscriptionPlanDto, SubscriptionPlan>().ReverseMap();
            CreateMap<UserSubscriptionDto, UserSubscription>().ReverseMap();
            CreateMap<UserShortDto, User>().ReverseMap();

            // Listening History
            CreateMap<ListeningHistory, ListeningHistoryEntryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ListenedAt, opt => opt.MapFrom(src => src.ListenedAt))
                .ForMember(dest => dest.DurationListenedMs, opt => opt.MapFrom(src => src.DurationListenedMs))
                .ForMember(dest => dest.Track, opt => opt.MapFrom(src => src.Track));

            // Playlist -> PlaylistDto 
            CreateMap<Playlist, PlaylistDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.OwnerUsername, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsPrivate, opt => opt.MapFrom(src => src.IsPrivate))
                .ForMember(dest => dest.CoverUrl, opt => opt.MapFrom(src => src.CoverUrl))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.TracksCount, opt => opt.MapFrom(src => src.PlaylistTracks != null ? src.PlaylistTracks.Count : 0))
                .ForMember(dest => dest.TotalDurationMs, opt => opt.MapFrom(src => src.PlaylistTracks != null ? src.PlaylistTracks.Sum(pt => pt.Track.DurationMs) : 0))
                .ForMember(dest => dest.IsOwner, opt => opt.Ignore());

            CreateMap<PlaylistDto, LibraryItemDto>()
                .ForMember(dest => dest.RouteKey, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Subtitle, opt => opt.MapFrom(src => $"{src.TracksCount} songs"))
                .ForMember(dest => dest.AudioUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Kind, opt => opt.MapFrom(_ => "playlist"));

            // Podcast
            CreateMap<Podcast, PodcastDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Username : string.Empty));

            CreateMap<Podcast, PodcastDetailsDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Username : string.Empty))
                .ForMember(dest => dest.AuthorAvatarUrl, opt => opt.MapFrom(src => src.Author != null ? src.Author.AvatarUrl : null))
                .ForMember(dest => dest.TotalDurationMs, opt => opt.MapFrom(src => src.Episodes != null ? src.Episodes.Sum(e => e.DurationMs) : 0));

            CreateMap<PodcastDto, LibraryItemDto>()
                .ForMember(dest => dest.RouteKey, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Subtitle, opt => opt.MapFrom(src => $"{src.Description} episodes")) 
                .ForMember(dest => dest.CoverUrl, opt => opt.MapFrom(src => src.CoverUrl))
                .ForMember(dest => dest.AudioUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Kind, opt => opt.MapFrom(_ => "podcast"));

            CreateMap<PodcastEpisode, PodcastEpisodeDto>();

            CreateMap<Podcast, LibraryItemDto>()
                .ForMember(dest => dest.RouteKey, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Subtitle, opt => opt.MapFrom(src => $"{src.Episodes.Count} episodes"))
                .ForMember(dest => dest.AudioUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Kind, opt => opt.MapFrom(_ => "podcast"));

            // Album -> LibraryItem
            CreateMap<Album, LibraryItemDto>()
                .ForMember(dest => dest.RouteKey, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Subtitle, opt => opt.MapFrom(src => src.Artist != null ? src.Artist.Name : string.Empty))
                .ForMember(dest => dest.AudioUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Kind, opt => opt.MapFrom(_ => "album"));

            // Artist -> LibraryItem
            CreateMap<Artist, LibraryItemDto>()
                .ForMember(dest => dest.RouteKey, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Subtitle, opt => opt.MapFrom(src => src.User != null && src.User.Followers != null ? $"{src.User.Followers.Count} followers" : "0 followers"))
                .ForMember(dest => dest.CoverUrl, opt => opt.MapFrom(src => src.AvatarUrl))
                .ForMember(dest => dest.AudioUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Kind, opt => opt.MapFrom(_ => "artist"));

            // User & Profile
            CreateMap<User, ProfileDto>()
                .ForMember(dest => dest.CountPlaylist, opt => opt.MapFrom(src => src.Playlists != null ? src.Playlists.Count : 0))
                .ForMember(dest => dest.CountFollowers, opt => opt.MapFrom(src => src.Followers != null ? src.Followers.Count : 0))
                .ForMember(dest => dest.IsFollowing, opt => opt.Ignore())
                .ForMember(dest => dest.Playlists, opt => opt.MapFrom(src => src.Playlists))
                .ForMember(dest => dest.History, opt => opt.MapFrom(src =>
                    src.ListeningHistories != null ? src.ListeningHistories.OrderByDescending(h => h.ListenedAt) : null));

            CreateMap<UpdateProfileDto, User>().ReverseMap();
        }
    }
}