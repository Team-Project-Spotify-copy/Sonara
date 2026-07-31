using AutoMapper;
using Application.DTOs.Subscription;

using Domain.Entities.Users;

namespace BusinessLogic.Configurations
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // Subscription
            CreateMap<CreateSubscriptionDto, Subscription>();
            CreateMap<UpdateSubscriptionDto, Subscription>();
            CreateMap<SubscriptionDto, Subscription>().ReverseMap();
        }
    }
}