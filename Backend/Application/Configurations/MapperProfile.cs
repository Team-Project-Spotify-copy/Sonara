using Application.DTOs.Music;
using Application.DTOs.Playlists;
using Application.DTOs.Subscription;
using Application.DTOs.Users;
using AutoMapper;
using Domain.Entities.Music;
using Domain.Entities.Playlists;
using Domain.Entities.Users;

namespace BusinessLogic.Configurations
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // Subscription
            CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>();
            CreateMap<UpdateSubscriptionPlanDto, SubscriptionPlan>();
            CreateMap<SubscriptionPlanDto, SubscriptionPlan>().ReverseMap();

            CreateMap<UserSubscriptionDto, UserSubscription>().ReverseMap();

            CreateMap<UserShortDto, User>().ReverseMap();

            // Track & History Mappings
            CreateMap<Track, TrackDto>()
                .ForMember(dest => dest.ArtworkUrl, opt => opt.MapFrom(src =>
                    src.Album != null && src.Album.CoverUrl != null
                        ? src.Album.CoverUrl
                        : (src.Artist != null ? src.Artist.AvatarUrl : null)));

            CreateMap<ListeningHistory, ListeningHistoryEntryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ListenedAt, opt => opt.MapFrom(src => src.ListenedAt))
                .ForMember(dest => dest.DurationListenedMs, opt => opt.MapFrom(src => src.DurationListenedMs))
                .ForMember(dest => dest.Track, opt => opt.MapFrom(src => src.Track));

            // Playlist
            CreateMap<Playlist, PlaylistDto>()
                .ConstructUsing(src => new PlaylistDto(
                    src.Id,
                    src.UserId,
                    src.User != null ? src.User.Username : string.Empty,
                    src.Name,
                    src.Description,
                    src.IsPrivate,
                    src.CoverUrl,
                    src.CreatedAt,
                    src.PlaylistTracks.Count,
                    src.PlaylistTracks.Sum(pt => pt.Track.DurationMs),
                    false
                ));

            // User & Profile
            CreateMap<User, ProfileDto>()
                .ForMember(dest => dest.CountPlaylist, opt => opt.MapFrom(src => src.Playlists.Count))
                .ForMember(dest => dest.CountFollowers, opt => opt.MapFrom(src => src.Followers.Count))
                .ForMember(dest => dest.IsFollowing, opt => opt.Ignore())
                .ForMember(dest => dest.Playlists, opt => opt.MapFrom(src => src.Playlists))
                .ForMember(dest => dest.History, opt => opt.MapFrom(src => src.ListeningHistories));

            CreateMap<UpdateProfileDto, User>().ReverseMap();
        }
    }
}