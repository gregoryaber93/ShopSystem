using OrderService.Application.Abstractions.Integrations;
using Grpc.Core;
using ShopSystem.Contracts.Grpc.Products;

namespace OrderService.Infrastructure.ProductCatalog;

public sealed class ProductCatalogGrpcGateway(ProductsGrpc.ProductsGrpcClient client) : IProductCatalogGateway
{
    public async Task<ProductSnapshot?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        GetProductByIdResponse response;
        try
        {
            response = await client.GetProductByIdAsync(
                new GetProductByIdRequest { ProductId = productId.ToString() },
                cancellationToken: cancellationToken);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.DeadlineExceeded)
        {
            throw new InvalidOperationException("ProductService request timed out.", exception);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.Unavailable)
        {
            throw new InvalidOperationException("ProductService is unavailable.", exception);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.Unauthenticated)
        {
            throw new InvalidOperationException("ProductService rejected service authentication.", exception);
        }

        if (!response.Found)
        {
            return null;
        }

        if (!Guid.TryParse(response.ProductId, out var parsedProductId) || parsedProductId == Guid.Empty)
        {
            throw new ArgumentException("ProductService returned invalid ProductId.");
        }

        if (!Guid.TryParse(response.ShopId, out var parsedShopId) || parsedShopId == Guid.Empty)
        {
            throw new ArgumentException("ProductService returned invalid ShopId.");
        }

        return new ProductSnapshot(
            parsedProductId,
            parsedShopId,
            decimal.Round(Convert.ToDecimal(response.Price), 2, MidpointRounding.AwayFromZero),
            response.Name,
            response.Type);
    }
}
