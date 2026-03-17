using Microsoft.Extensions.DependencyInjection;
using PromotionService.Application.Abstractions.Caching;
using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Features.Promotions.Commands.AddPromotion;
using PromotionService.Application.Features.Promotions.Commands.DeletePromotion;
using PromotionService.Application.Features.Promotions.Commands.UpsertUserPromotionProfile;
using PromotionService.Application.Features.Promotions.Commands.UpdatePromotion;
using PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions;
using PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;
using PromotionService.Application.Features.Promotions.Queries.GetPromotions;
using PromotionService.Application.Features.Promotions.Queries.ReplayUserLoyaltyProjection;
using PromotionService.Application.Pipeline;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Evaluation pipeline filters — order is significant
        services.AddScoped<IPipelineFilter<EvaluationContext>, ValidationFilter>();
        services.AddScoped<IPipelineFilter<EvaluationContext>, NormalizationFilter>();
        services.AddScoped<IPipelineFilter<EvaluationContext>, CacheReadFilter>();
        services.AddScoped<IPipelineFilter<EvaluationContext>, DataLoadingFilter>();
        services.AddScoped<IPipelineFilter<EvaluationContext>, ActivePromotionFilter>();
        services.AddScoped<IPipelineFilter<EvaluationContext>, ProductDiscountFilter>();
        services.AddScoped<IPipelineFilter<EvaluationContext>, LoyaltyPointsFilter>();
        services.AddScoped<IPipelineFilter<EvaluationContext>, TotalCalculationFilter>();
        services.AddScoped<IPipelineFilter<EvaluationContext>, CacheWriteFilter>();
        services.AddScoped<PipelineRunner<EvaluationContext>>();

        services.AddScoped<IQueryHandler<GetPromotionsQuery, IReadOnlyCollection<PromotionDto>>, GetPromotionsQueryHandler>();
        services.AddScoped<IQueryHandler<EvaluatePromotionsQuery, PromotionEvaluationResultDto>, EvaluatePromotionsQueryHandler>();
        services.AddScoped<IQueryHandler<ReplayUserLoyaltyProjectionQuery, UserPromotionProfileDto?>, ReplayUserLoyaltyProjectionQueryHandler>();
        services.AddScoped<ICommandHandler<AddPromotionCommand, PromotionDto>, AddPromotionCommandHandler>();
        services.AddScoped<ICommandHandler<UpdatePromotionCommand, PromotionDto?>, UpdatePromotionCommandHandler>();
        services.AddScoped<ICommandHandler<DeletePromotionCommand, bool>, DeletePromotionCommandHandler>();
        services.AddScoped<ICommandHandler<UpsertUserPromotionProfileCommand, UserPromotionProfileDto>, UpsertUserPromotionProfileCommandHandler>();

        return services;
    }
}
