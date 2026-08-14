using AutoMapper;
using Application.DTOs.Subscription;
using Application.DTOs.Users;

using Domain.Entities.Users;

namespace BusinessLogic.Configurations
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateSubscriptionDto, Subscription>();
            CreateMap<UpdateSubscriptionDto, Subscription>();
            CreateMap<SubscriptionDto, Subscription>().ReverseMap();

            CreateMap<ProfileDto, User>().ReverseMap();
            CreateMap<UpdateProfileDto, User>().ReverseMap();
        }
    }
}
