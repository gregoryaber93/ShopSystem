using ProductService.Application.Abstractions.CQRS;
using ProductService.Application.Abstractions.Persistence;
using ProductService.Application.Abstractions.Security;
using ProductService.Application.Abstractions.Shops;
using ProductService.Contracts.Dtos;
using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Products.Commands.AddProduct;

public sealed class AddProductCommandHandler(
    IProductRepository productRepository,
    IShopOwnershipClient shopOwnershipClient,
    ICurrentUserService currentUserService) : ICommandHandler<AddProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(AddProductCommand command, CancellationToken cancellationToken)
    {
        ValidateProduct(command.Product);

        if (currentUserService.IsInRole("Manager"))
        {
            var currentUserId = currentUserService.GetUserIdOrThrow();
            var ownerUserId = await shopOwnershipClient.GetShopOwnerAsync(command.Product.ShopId, cancellationToken);
            if (ownerUserId != currentUserId)
            {
                throw new UnauthorizedAccessException("Manager can only add products to their own shops.");
            }
        }

        var product = new ProductEntity
        {
            Id = command.Product.Id == Guid.Empty ? Guid.NewGuid() : command.Product.Id,
            Name = command.Product.Name.Trim(),
            Type = command.Product.Type.Trim(),
            Price = command.Product.Price,
            ShopId = command.Product.ShopId
        };

        await productRepository.AddAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);

        return new ProductDto(product.Id, product.Name, product.Type, product.Price, product.ShopId);
    }

    private static void ValidateProduct(ProductDto product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ArgumentException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(product.Type))
        {
            throw new ArgumentException("Type is required.");
        }

        if (product.Price < 0)
        {
            throw new ArgumentException("Price cannot be negative.");
        }

        if (product.ShopId == Guid.Empty)
        {
            throw new ArgumentException("ShopId is required.");
        }
    }
}
