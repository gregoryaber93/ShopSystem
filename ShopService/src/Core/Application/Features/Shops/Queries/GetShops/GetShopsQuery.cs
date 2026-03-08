using ShopService.Application.Abstractions.CQRS;
using ShopService.Contracts.Dtos;

namespace ShopService.Application.Features.Shops.Queries.GetShops;

public sealed record GetShopsQuery : IQuery<IReadOnlyCollection<ShopDto>>;