using AutoMapper;
using HSMVC.Domain;
using HSMVC.Features.Conference.Commands;

namespace HSMVC.Infrastructure.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Conference, ConferenceEditCommand>();
            CreateMap<ConferenceEditCommand, Conference>();
            CreateMap<ConferenceAddCommand, Conference>();
        }
    }
}