using ShopService.Application.Abstractions.CQRS;
using ShopService.Application.Abstractions.Persistence;

namespace ShopService.Application.Features.Shops.Commands.DeleteShop;

public sealed class DeleteShopCommandHandler(IShopRepository shopRepository) : ICommandHandler<DeleteShopCommand, bool>
{
    public async Task<bool> Handle(DeleteShopCommand command, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetByIdAsync(command.Id, cancellationToken);
        if (shop is null)
        {
            return false;
        }

        shopRepository.Remove(shop);
        await shopRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}