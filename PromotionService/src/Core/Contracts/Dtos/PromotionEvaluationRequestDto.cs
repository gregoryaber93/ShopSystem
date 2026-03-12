namespace PromotionService.Contracts.Dtos;

public sealed record PromotionEvaluationRequestDto(
    Guid UserId,
    decimal Subtotal,
    IReadOnlyCollection<Guid>? ProductIds,
    DateTime? EvaluatedAtUtc = null);
