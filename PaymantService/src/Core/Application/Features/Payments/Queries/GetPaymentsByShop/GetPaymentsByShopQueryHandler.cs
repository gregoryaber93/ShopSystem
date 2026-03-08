using PaymantService.Application.Abstractions.CQRS;
using PaymantService.Application.Abstractions.Persistence;
using PaymantService.Contracts.Dtos;

namespace PaymantService.Application.Features.Payments.Queries.GetPaymentsByShop;

public sealed class GetPaymentsByShopQueryHandler(IPaymentRepository paymentRepository) : IQueryHandler<GetPaymentsByShopQuery, IReadOnlyCollection<PaymentDto>>
{
    public async Task<IReadOnlyCollection<PaymentDto>> Handle(GetPaymentsByShopQuery query, CancellationToken cancellationToken)
    {
        if (query.ShopId == Guid.Empty)
        {
            throw new ArgumentException("ShopId is required.");
        }

        var payments = await paymentRepository.GetByShopIdAsync(query.ShopId, cancellationToken);
        return payments
            .Select(payment => new PaymentDto(
                payment.Id,
                payment.UserId,
                payment.ShopId,
                payment.OrderId,
                payment.Amount,
                payment.Currency,
                payment.Method,
                payment.Status,
                payment.PaidAtUtc))
            .ToList();
    }
}


