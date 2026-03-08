using ShopService.Application.Abstractions.CQRS;
using ShopService.Contracts.Dtos;

namespace ShopService.Application.Features.Shops.Commands.AddShop;

public sealed record AddShopCommand(ShopDto Shop) : ICommand<ShopDto>;