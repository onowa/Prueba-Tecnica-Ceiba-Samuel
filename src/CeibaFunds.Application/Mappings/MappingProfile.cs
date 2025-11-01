using AutoMapper;
using CeibaFunds.Domain.Entities;
using CeibaFunds.Application.DTOs;

namespace CeibaFunds.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.GetFullName()));

        CreateMap<Fund, FundDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value));

        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId.Value))
            .ForMember(dest => dest.FundId, opt => opt.MapFrom(src => src.FundId.Value));

        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId.Value))
            .ForMember(dest => dest.FundId, opt => opt.MapFrom(src => src.FundId != null ? src.FundId.Value : null))
            .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId != null ? src.SubscriptionId.Value : null));
    }
}