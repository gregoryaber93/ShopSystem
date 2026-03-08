using Microsoft.Extensions.DependencyInjection;
using PaymantService.Application.Abstractions.CQRS;
using PaymantService.Application.Features.Payments.Commands.ProcessPayment;
using PaymantService.Application.Features.Payments.Queries.GetMyPayments;
using PaymantService.Application.Features.Payments.Queries.GetPaymentsByShop;
using PaymantService.Contracts.Dtos;

namespace PaymantService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<ProcessPaymentCommand, PaymentDto>, ProcessPaymentCommandHandler>();
        services.AddScoped<IQueryHandler<GetMyPaymentsQuery, IReadOnlyCollection<PaymentDto>>, GetMyPaymentsQueryHandler>();
        services.AddScoped<IQueryHandler<GetPaymentsByShopQuery, IReadOnlyCollection<PaymentDto>>, GetPaymentsByShopQueryHandler>();

        return services;
    }
}


