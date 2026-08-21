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
            // Subscription
            CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>();
            CreateMap<UpdateSubscriptionPlanDto, SubscriptionPlan>();
            CreateMap<SubscriptionPlanDto, SubscriptionPlan>().ReverseMap();

            CreateMap<UserSubscriptionDto, UserSubscription>().ReverseMap();

            // User
            CreateMap<ProfileDto, User>().ReverseMap();
            CreateMap<UpdateProfileDto, User>().ReverseMap();
        }
    }
}