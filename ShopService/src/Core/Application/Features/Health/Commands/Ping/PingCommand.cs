using ShopService.Application.Abstractions.CQRS;

namespace ShopService.Application.Features.Health.Commands.Ping;

public sealed record PingCommand : ICommand<bool>;
