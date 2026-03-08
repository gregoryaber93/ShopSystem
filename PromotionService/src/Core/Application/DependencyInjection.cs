using Microsoft.Extensions.DependencyInjection;
using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Features.Promotions.Commands.AddPromotion;
using PromotionService.Application.Features.Promotions.Commands.DeletePromotion;
using PromotionService.Application.Features.Promotions.Commands.UpdatePromotion;
using PromotionService.Application.Features.Promotions.Queries.GetPromotions;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetPromotionsQuery, IReadOnlyCollection<PromotionDto>>, GetPromotionsQueryHandler>();
        services.AddScoped<ICommandHandler<AddPromotionCommand, PromotionDto>, AddPromotionCommandHandler>();
        services.AddScoped<ICommandHandler<UpdatePromotionCommand, PromotionDto?>, UpdatePromotionCommandHandler>();
        services.AddScoped<ICommandHandler<DeletePromotionCommand, bool>, DeletePromotionCommandHandler>();

        return services;
    }
}
