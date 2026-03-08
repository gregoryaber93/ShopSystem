using ShopService.Application.Abstractions.CQRS;

namespace ShopService.Application.Features.Shops.Commands.DeleteShop;

public sealed record DeleteShopCommand(Guid Id) : ICommand<bool>;