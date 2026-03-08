using ProductService.Application.Abstractions.CQRS;
using ProductService.Application.Abstractions.Persistence;

namespace ProductService.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(IProductRepository productRepository) : ICommandHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        productRepository.Remove(product);
        await productRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
