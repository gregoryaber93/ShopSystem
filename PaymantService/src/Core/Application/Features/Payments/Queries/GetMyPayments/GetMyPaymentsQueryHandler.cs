using PaymantService.Application.Abstractions.CQRS;
using PaymantService.Application.Abstractions.Persistence;
using PaymantService.Contracts.Dtos;

namespace PaymantService.Application.Features.Payments.Queries.GetMyPayments;

public sealed class GetMyPaymentsQueryHandler(IPaymentRepository paymentRepository) : IQueryHandler<GetMyPaymentsQuery, IReadOnlyCollection<PaymentDto>>
{
    public async Task<IReadOnlyCollection<PaymentDto>> Handle(GetMyPaymentsQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.");
        }

        var payments = await paymentRepository.GetByUserIdAsync(query.UserId, cancellationToken);
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


