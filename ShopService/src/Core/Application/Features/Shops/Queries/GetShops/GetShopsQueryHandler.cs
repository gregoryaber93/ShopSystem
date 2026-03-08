using ShopService.Application.Abstractions.CQRS;
using ShopService.Application.Abstractions.Persistence;
using ShopService.Contracts.Dtos;

namespace ShopService.Application.Features.Shops.Queries.GetShops;

public sealed class GetShopsQueryHandler(IShopRepository shopRepository) : IQueryHandler<GetShopsQuery, IReadOnlyCollection<ShopDto>>
{
    public async Task<IReadOnlyCollection<ShopDto>> Handle(GetShopsQuery query, CancellationToken cancellationToken)
    {
        var shops = await shopRepository.GetAllAsync(cancellationToken);

        return shops
            .Select(shop => new ShopDto(shop.Id, shop.Name, shop.Code))
            .ToArray();
    }
}