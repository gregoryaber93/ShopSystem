using ShopService.Application.Abstractions.CQRS;
using ShopService.Application.Abstractions.Persistence;
using ShopService.Application.Abstractions.Security;
using ShopService.Contracts.Dtos;
using ShopService.Domain.Entities;

namespace ShopService.Application.Features.Shops.Commands.AddShop;

public sealed class AddShopCommandHandler(
    IShopRepository shopRepository,
    ICurrentUserService currentUserService) : ICommandHandler<AddShopCommand, ShopDto>
{
    public async Task<ShopDto> Handle(AddShopCommand command, CancellationToken cancellationToken)
    {
        var request = command.Shop;

        Guid? ownerUserId = currentUserService.IsInRole("Manager")
            ? currentUserService.GetUserIdOrThrow()
            : null;

        var shop = new ShopEntity
        {
            Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id,
            Name = request.Name,
            Code = request.Code,
            OwnerUserId = ownerUserId
        };

        await shopRepository.AddAsync(shop, cancellationToken);
        await shopRepository.SaveChangesAsync(cancellationToken);

        return new ShopDto(shop.Id, shop.Name, shop.Code);
    }
}