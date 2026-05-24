using AutoMapper;
using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Mappings;

public sealed class PromotionMappingsProfile : Profile
{
    public PromotionMappingsProfile()
    {
        CreateMap<PromotionType, PromotionTypeDto>()
            .ConvertUsing(type => (PromotionTypeDto)(int)type);

        CreateMap<PromotionEntity, PromotionDto>();
    }
}
