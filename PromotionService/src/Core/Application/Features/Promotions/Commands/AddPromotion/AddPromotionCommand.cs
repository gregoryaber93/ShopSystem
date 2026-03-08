using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Features.Promotions.Commands.AddPromotion;

public sealed record AddPromotionCommand(PromotionDto Promotion) : ICommand<PromotionDto>;
