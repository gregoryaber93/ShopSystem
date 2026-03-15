namespace OrderService.Application.Abstractions.Integrations;

public interface IPromotionGateway
{
    Task<PromotionEvaluationSnapshot> EvaluateAsync(
        Guid userId,
        decimal subtotal,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);
}
