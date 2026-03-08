using PaymantService.Application.Abstractions.CQRS;
using PaymantService.Contracts.Dtos;

namespace PaymantService.Application.Features.Payments.Commands.ProcessPayment;

public sealed record ProcessPaymentCommand(Guid UserId, ProcessPaymentRequestDto Request) : ICommand<PaymentDto>;


