using ShopService.Application.Abstractions.CQRS;
using ShopService.Application.Abstractions.Persistence;
using ShopService.Application.Abstractions.Security;
using ShopService.Contracts.Dtos;

namespace ShopService.Application.Features.Shops.Commands.UpdateShop;

public sealed class UpdateShopCommandHandler(
    IShopRepository shopRepository,
    ICurrentUserService currentUserService) : ICommandHandler<UpdateShopCommand, ShopDto?>
{
    public async Task<ShopDto?> Handle(UpdateShopCommand command, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetByIdAsync(command.Id, cancellationToken);
        if (shop is null)
        {
            return null;
        }

        if (currentUserService.IsInRole("Manager"))
        {
            var currentUserId = currentUserService.GetUserIdOrThrow();
            if (shop.OwnerUserId != currentUserId)
            {
                throw new UnauthorizedAccessException("Manager moze aktualizowac tylko wlasne sklepy.");
            }
        }

        shop.Name = command.Shop.Name;
        shop.Code = command.Shop.Code;

        await shopRepository.SaveChangesAsync(cancellationToken);

        return new ShopDto(shop.Id, shop.Name, shop.Code);
    }
}