using Grpc.Core;
using ProductService.Application.Abstractions.CQRS;
using ProductService.Application.Features.Products.Queries.GetProductById;
using ShopSystem.Contracts.Grpc.Products;

namespace ProductService.Api.Grpc;

public sealed class ProductsGrpcService(
    IQueryHandler<GetProductByIdQuery, Contracts.Dtos.ProductDto?> getProductByIdQueryHandler) : ProductsGrpc.ProductsGrpcBase
{
    public override async Task<GetProductByIdResponse> GetProductById(GetProductByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId) || productId == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProductId is invalid."));
        }

        var product = await getProductByIdQueryHandler.Handle(new GetProductByIdQuery(productId), context.CancellationToken);
        if (product is null)
        {
            return new GetProductByIdResponse { Found = false };
        }

        return new GetProductByIdResponse
        {
            Found = true,
            ProductId = product.Id.ToString(),
            ShopId = product.ShopId.ToString(),
            Price = decimal.ToDouble(product.Price),
            Name = product.Name,
            Type = product.Type
        };
    }
}
