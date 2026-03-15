using Grpc.Core;
using OrderService.Application.Abstractions.Integrations;
using ShopSystem.Contracts.Grpc.Promotions;

namespace OrderService.Infrastructure.Promotion;

public sealed class PromotionGrpcGateway(PromotionsGrpc.PromotionsGrpcClient client) : IPromotionGateway
{
    public async Task<PromotionEvaluationSnapshot> EvaluateAsync(
        Guid userId,
        decimal subtotal,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        EvaluatePromotionsResponse response;
        try
        {
            var request = new EvaluatePromotionsRequest
            {
                UserId = userId.ToString("D"),
                Subtotal = decimal.ToDouble(subtotal)
            };

            request.ProductIds.AddRange(productIds.Select(id => id.ToString("D")));

            response = await client.EvaluatePromotionsAsync(request, cancellationToken: cancellationToken);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.DeadlineExceeded)
        {
            throw new InvalidOperationException("PromotionService request timed out.", exception);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.Unavailable)
        {
            throw new InvalidOperationException("PromotionService is unavailable.", exception);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.Unauthenticated)
        {
            throw new InvalidOperationException("PromotionService rejected service authentication.", exception);
        }

        var applied = response.AppliedPromotions
            .Select(item => new AppliedPromotionSnapshot(
                Guid.TryParse(item.PromotionId, out var parsed) ? parsed : Guid.Empty,
                item.Type.ToString(),
                decimal.Round(Convert.ToDecimal(item.DiscountPercentage), 2, MidpointRounding.AwayFromZero),
                item.Reason))
            .Where(item => item.PromotionId != Guid.Empty)
            .ToArray();

        return new PromotionEvaluationSnapshot(
            userId,
            decimal.Round(Convert.ToDecimal(response.Subtotal), 2, MidpointRounding.AwayFromZero),
            decimal.Round(Convert.ToDecimal(response.DiscountAmount), 2, MidpointRounding.AwayFromZero),
            decimal.Round(Convert.ToDecimal(response.FinalPrice), 2, MidpointRounding.AwayFromZero),
            applied);
    }
}
