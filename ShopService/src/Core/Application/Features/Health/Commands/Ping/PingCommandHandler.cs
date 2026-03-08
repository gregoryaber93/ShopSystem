using ShopService.Application.Abstractions.CQRS;

namespace ShopService.Application.Features.Health.Commands.Ping;

public sealed class PingCommandHandler : ICommandHandler<PingCommand, bool>
{
    public Task<bool> Handle(PingCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
