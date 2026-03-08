using PaymantService.Application.Abstractions.CQRS;
using PaymantService.Contracts.Dtos;

namespace PaymantService.Application.Features.Payments.Queries.GetPaymentsByShop;

public sealed record GetPaymentsByShopQuery(Guid ShopId) : IQuery<IReadOnlyCollection<PaymentDto>>;


