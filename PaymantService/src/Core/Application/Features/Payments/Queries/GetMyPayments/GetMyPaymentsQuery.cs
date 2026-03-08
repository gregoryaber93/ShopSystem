using PaymantService.Application.Abstractions.CQRS;
using PaymantService.Contracts.Dtos;

namespace PaymantService.Application.Features.Payments.Queries.GetMyPayments;

public sealed record GetMyPaymentsQuery(Guid UserId) : IQuery<IReadOnlyCollection<PaymentDto>>;


