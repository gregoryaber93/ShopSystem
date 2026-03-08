using PromotionService.Application.Abstractions.CQRS;

namespace PromotionService.Application.Features.Promotions.Commands.DeletePromotion;

public sealed record DeletePromotionCommand(Guid Id) : ICommand<bool>;
