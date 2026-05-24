using AutoMapper;
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
    IQueryHandler<EvaluatePromotionsQuery, PromotionEvaluationResultDto> evaluatePromotionsQueryHandler,
    IMapper mapper) : PromotionsGrpc.PromotionsGrpcBase
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
        response.Promotions.AddRange(filtered.Select(mapper.Map<ShopSystem.Contracts.Grpc.Promotions.PromotionDto>));
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

        var response = mapper.Map<EvaluatePromotionsResponse>(evaluation);
        response.AppliedPromotions.AddRange(evaluation.AppliedPromotions.Select(mapper.Map<ShopSystem.Contracts.Grpc.Promotions.AppliedPromotionDto>));

        return response;
    }

    private static bool IsActive(PromotionService.Contracts.Dtos.PromotionDto promotion, DateTime evaluatedAtUtc)
    {
        var started = promotion.StartsAtUtc is null || promotion.StartsAtUtc.Value <= evaluatedAtUtc;
        var notEnded = promotion.EndsAtUtc is null || promotion.EndsAtUtc.Value >= evaluatedAtUtc;
        return started && notEnded;
    }

}
