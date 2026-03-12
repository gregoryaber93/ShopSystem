using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Features.Promotions.Commands.UpsertUserPromotionProfile;

public sealed record UpsertUserPromotionProfileCommand(UserPromotionProfileDto Profile) : ICommand<UserPromotionProfileDto>;
