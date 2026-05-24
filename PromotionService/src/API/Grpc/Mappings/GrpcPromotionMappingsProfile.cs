using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using PromotionService.Contracts.Dtos;
using GrpcPromotionDto = ShopSystem.Contracts.Grpc.Promotions.PromotionDto;
using GrpcAppliedPromotionDto = ShopSystem.Contracts.Grpc.Promotions.AppliedPromotionDto;
using GrpcPromotionType = ShopSystem.Contracts.Grpc.Promotions.PromotionType;
using GrpcEvaluatePromotionsResponse = ShopSystem.Contracts.Grpc.Promotions.EvaluatePromotionsResponse;

namespace PromotionService.Api.Grpc.Mappings;

public sealed class GrpcPromotionMappingsProfile : Profile
{
    public GrpcPromotionMappingsProfile()
    {
        CreateMap<PromotionTypeDto, GrpcPromotionType>()
            .ConvertUsing(type => (GrpcPromotionType)(int)type);

        CreateMap<PromotionService.Contracts.Dtos.PromotionDto, GrpcPromotionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.DiscountPercentage, opt => opt.MapFrom(src => decimal.ToDouble(src.DiscountPercentage)))
            .ForMember(dest => dest.RequiredPoints, opt => opt.MapFrom(src => src.RequiredPoints.HasValue ? decimal.ToDouble(src.RequiredPoints.Value) : 0d))
            .ForMember(dest => dest.StartsAtUtc, opt => opt.MapFrom(src => src.StartsAtUtc.HasValue
                ? Timestamp.FromDateTime(DateTime.SpecifyKind(src.StartsAtUtc.Value, DateTimeKind.Utc))
                : null))
            .ForMember(dest => dest.EndsAtUtc, opt => opt.MapFrom(src => src.EndsAtUtc.HasValue
                ? Timestamp.FromDateTime(DateTime.SpecifyKind(src.EndsAtUtc.Value, DateTimeKind.Utc))
                : null))
            .ForMember(dest => dest.ProductIds, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                if (src.ProductIds is null)
                {
                    return;
                }

                dest.ProductIds.AddRange(src.ProductIds.Select(id => id.ToString()));
            });

        CreateMap<AppliedPromotionDto, GrpcAppliedPromotionDto>()
            .ForMember(dest => dest.PromotionId, opt => opt.MapFrom(src => src.PromotionId.ToString()))
            .ForMember(dest => dest.DiscountPercentage, opt => opt.MapFrom(src => decimal.ToDouble(src.DiscountPercentage)));

        CreateMap<PromotionEvaluationResultDto, GrpcEvaluatePromotionsResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => decimal.ToDouble(src.Subtotal)))
            .ForMember(dest => dest.TotalDiscountPercentage, opt => opt.MapFrom(src => decimal.ToDouble(src.TotalDiscountPercentage)))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => decimal.ToDouble(src.DiscountAmount)))
            .ForMember(dest => dest.FinalPrice, opt => opt.MapFrom(src => decimal.ToDouble(src.FinalPrice)))
            .ForMember(dest => dest.AppliedPromotions, opt => opt.Ignore());
    }
}
