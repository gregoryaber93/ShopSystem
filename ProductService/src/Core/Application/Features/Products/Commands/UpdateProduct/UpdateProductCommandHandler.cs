using ProductService.Application.Abstractions.CQRS;
using ProductService.Application.Abstractions.Persistence;
using ProductService.Application.Abstractions.Security;
using ProductService.Application.Abstractions.Shops;
using ProductService.Contracts.Dtos;

namespace ProductService.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IShopOwnershipClient shopOwnershipClient,
    ICurrentUserService currentUserService) : ICommandHandler<UpdateProductCommand, ProductDto?>
{
    public async Task<ProductDto?> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        if (currentUserService.IsInRole("Manager"))
        {
            var currentUserId = currentUserService.GetUserIdOrThrow();
            var ownerUserId = await shopOwnershipClient.GetShopOwnerAsync(product.ShopId, cancellationToken);
            if (ownerUserId != currentUserId)
            {
                throw new UnauthorizedAccessException("Manager can only update products in their own shops.");
            }
        }

        ValidateProduct(command.Product);

        product.Name = command.Product.Name.Trim();
        product.Type = command.Product.Type.Trim();
        product.Price = command.Product.Price;
        product.ShopId = command.Product.ShopId;

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
