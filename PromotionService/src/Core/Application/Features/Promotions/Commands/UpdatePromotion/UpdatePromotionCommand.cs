using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Features.Promotions.Commands.UpdatePromotion;

public sealed record UpdatePromotionCommand(Guid Id, PromotionDto Promotion) : ICommand<PromotionDto?>;
