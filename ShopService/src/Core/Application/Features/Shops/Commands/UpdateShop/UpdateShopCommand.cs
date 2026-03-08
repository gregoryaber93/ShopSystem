using ShopService.Application.Abstractions.CQRS;
using ShopService.Contracts.Dtos;

namespace ShopService.Application.Features.Shops.Commands.UpdateShop;

public sealed record UpdateShopCommand(Guid Id, ShopDto Shop) : ICommand<ShopDto?>;