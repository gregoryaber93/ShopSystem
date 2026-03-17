using ShopService.Application.Abstractions.CQRS;
using ShopService.Application.Abstractions.Persistence;
using ShopService.Application.Abstractions.Security;

namespace ShopService.Application.Features.Shops.Commands.DeleteShop;

public sealed class DeleteShopCommandHandler(
    IShopRepository shopRepository,
    ICurrentUserService currentUserService) : ICommandHandler<DeleteShopCommand, bool>
{
    public async Task<bool> Handle(DeleteShopCommand command, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetByIdAsync(command.Id, cancellationToken);
        if (shop is null)
        {
            return false;
        }

        if (currentUserService.IsInRole("Manager"))
        {
            var currentUserId = currentUserService.GetUserIdOrThrow();
            if (shop.OwnerUserId != currentUserId)
            {
                throw new UnauthorizedAccessException("Manager moze usuwac tylko wlasne sklepy.");
            }
        }

        shopRepository.Remove(shop);
        await shopRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}