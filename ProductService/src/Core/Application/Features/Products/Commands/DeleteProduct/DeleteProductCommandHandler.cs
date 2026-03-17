using ProductService.Application.Abstractions.CQRS;
using ProductService.Application.Abstractions.Persistence;
using ProductService.Application.Abstractions.Security;
using ProductService.Application.Abstractions.Shops;

namespace ProductService.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IShopOwnershipClient shopOwnershipClient,
    ICurrentUserService currentUserService) : ICommandHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        if (currentUserService.IsInRole("Manager"))
        {
            var currentUserId = currentUserService.GetUserIdOrThrow();
            var ownerUserId = await shopOwnershipClient.GetShopOwnerAsync(product.ShopId, cancellationToken);
            if (ownerUserId != currentUserId)
            {
                throw new UnauthorizedAccessException("Manager can only delete products in their own shops.");
            }
        }

        productRepository.Remove(product);
        await productRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
