using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions;
using PromotionService.Application.Features.Promotions.Queries.GetPromotions;
using PromotionService.Contracts.Dtos;
using ShopSystem.Contracts.Grpc.Promotions;

namespace PromotionService.Api.Grpc;

public sealed class PromotionsGrpcService(
    IQueryHandler<GetPromotionsQuery, IReadOnlyCollection<PromotionService.Contracts.Dtos.PromotionDto>> getPromotionsQueryHandler,
    IQueryHandler<EvaluatePromotionsQuery, PromotionEvaluationResultDto> evaluatePromotionsQueryHandler) : PromotionsGrpc.PromotionsGrpcBase
{
    public override async Task<GetPromotionsResponse> GetPromotions(GetPromotionsRequest request, ServerCallContext context)
    {
        var promotions = await getPromotionsQueryHandler.Handle(new GetPromotionsQuery(), context.CancellationToken);

        var evaluatedAtUtc = request.EvaluatedAtUtc is null
            ? DateTime.UtcNow
            : request.EvaluatedAtUtc.ToDateTime().ToUniversalTime();

        var filtered = request.OnlyActive
            ? promotions.Where(p => IsActive(p, evaluatedAtUtc)).ToArray()
            : promotions;

        var response = new GetPromotionsResponse();
        response.Promotions.AddRange(filtered.Select(MapPromotion));
        return response;
    }

    public override async Task<EvaluatePromotionsResponse> EvaluatePromotions(EvaluatePromotionsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId) || userId == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is invalid."));
        }

        var productIds = request.ProductIds
            .Select(id => Guid.TryParse(id, out var parsed) ? parsed : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToArray();

        var evaluatedAtUtc = request.EvaluatedAtUtc is null
            ? (DateTime?)null
            : request.EvaluatedAtUtc.ToDateTime().ToUniversalTime();

        var evaluation = await evaluatePromotionsQueryHandler.Handle(
            new EvaluatePromotionsQuery(new PromotionEvaluationRequestDto(
                userId,
                decimal.Round(Convert.ToDecimal(request.Subtotal), 2, MidpointRounding.AwayFromZero),
                productIds,
                evaluatedAtUtc)),
            context.CancellationToken);

        var response = new EvaluatePromotionsResponse
        {
            UserId = evaluation.UserId.ToString(),
            Subtotal = decimal.ToDouble(evaluation.Subtotal),
            TotalDiscountPercentage = decimal.ToDouble(evaluation.TotalDiscountPercentage),
            DiscountAmount = decimal.ToDouble(evaluation.DiscountAmount),
            FinalPrice = decimal.ToDouble(evaluation.FinalPrice)
        };

        response.AppliedPromotions.AddRange(evaluation.AppliedPromotions.Select(applied => new ShopSystem.Contracts.Grpc.Promotions.AppliedPromotionDto
        {
            PromotionId = applied.PromotionId.ToString(),
            Type = MapType(applied.Type),
            DiscountPercentage = decimal.ToDouble(applied.DiscountPercentage),
            Reason = applied.Reason
        }));

        return response;
    }

    private static bool IsActive(PromotionService.Contracts.Dtos.PromotionDto promotion, DateTime evaluatedAtUtc)
    {
        var started = promotion.StartsAtUtc is null || promotion.StartsAtUtc.Value <= evaluatedAtUtc;
        var notEnded = promotion.EndsAtUtc is null || promotion.EndsAtUtc.Value >= evaluatedAtUtc;
        return started && notEnded;
    }

    private static ShopSystem.Contracts.Grpc.Promotions.PromotionDto MapPromotion(PromotionService.Contracts.Dtos.PromotionDto promotion)
    {
        var mapped = new ShopSystem.Contracts.Grpc.Promotions.PromotionDto
        {
            Id = promotion.Id.ToString(),
            Type = MapType(promotion.Type),
            DiscountPercentage = decimal.ToDouble(promotion.DiscountPercentage),
            RequiredPoints = promotion.RequiredPoints is null ? 0 : decimal.ToDouble(promotion.RequiredPoints.Value)
        };

        if (promotion.StartsAtUtc is not null)
        {
            mapped.StartsAtUtc = Timestamp.FromDateTime(DateTime.SpecifyKind(promotion.StartsAtUtc.Value, DateTimeKind.Utc));
        }

        if (promotion.EndsAtUtc is not null)
        {
            mapped.EndsAtUtc = Timestamp.FromDateTime(DateTime.SpecifyKind(promotion.EndsAtUtc.Value, DateTimeKind.Utc));
        }

        mapped.ProductIds.AddRange((promotion.ProductIds ?? []).Select(id => id.ToString()));
        return mapped;
    }

    private static PromotionType MapType(PromotionTypeDto type)
    {
        return type switch
        {
            PromotionTypeDto.ProductDiscount => PromotionType.ProductDiscount,
            PromotionTypeDto.LoyaltyPoints => PromotionType.LoyaltyPoints,
            _ => (PromotionType)0
        };
    }
}
