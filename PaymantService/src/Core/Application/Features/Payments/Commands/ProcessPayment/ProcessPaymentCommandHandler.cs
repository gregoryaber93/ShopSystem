using PaymantService.Application.Abstractions.CQRS;
using PaymantService.Application.Abstractions.Persistence;
using PaymantService.Contracts.Dtos;
using PaymantService.Domain.Entities;

namespace PaymantService.Application.Features.Payments.Commands.ProcessPayment;

public sealed class ProcessPaymentCommandHandler(IPaymentRepository paymentRepository) : ICommandHandler<ProcessPaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        ValidateRequest(command.UserId, command.Request);

        var normalizedAmount = decimal.Round(command.Request.Amount, 2, MidpointRounding.AwayFromZero);
        var normalizedCurrency = command.Request.Currency.Trim().ToUpperInvariant();
        var normalizedMethod = command.Request.Method.Trim();

        var payment = new PaymentEntity
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            ShopId = command.Request.ShopId,
            OrderId = command.Request.OrderId,
            Amount = normalizedAmount,
            Currency = normalizedCurrency,
            Method = normalizedMethod,
            Status = "Completed",
            PaidAtUtc = DateTime.UtcNow
        };

        await paymentRepository.AddAsync(payment, cancellationToken);
        await paymentRepository.SaveChangesAsync(cancellationToken);

        return ToDto(payment);
    }

    private static void ValidateRequest(Guid userId, ProcessPaymentRequestDto request)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.");
        }

        if (request.ShopId == Guid.Empty)
        {
            throw new ArgumentException("ShopId is required.");
        }

        if (request.OrderId == Guid.Empty)
        {
            throw new ArgumentException("OrderId is required.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Trim().Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-letter code, e.g. PLN.");
        }

        if (string.IsNullOrWhiteSpace(request.Method))
        {
            throw new ArgumentException("Payment method is required.");
        }
    }

    private static PaymentDto ToDto(PaymentEntity payment)
    {
        return new PaymentDto(
            payment.Id,
            payment.UserId,
            payment.ShopId,
            payment.OrderId,
            payment.Amount,
            payment.Currency,
            payment.Method,
            payment.Status,
            payment.PaidAtUtc);
    }
}


