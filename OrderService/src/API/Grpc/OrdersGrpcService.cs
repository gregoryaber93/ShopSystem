using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Features.Orders.Queries.GetOrderById;
using ShopSystem.Contracts.Grpc.Orders;

namespace OrderService.Api.Grpc;

public sealed class OrdersGrpcService(
    IQueryHandler<GetOrderByIdQuery, OrderService.Contracts.Dtos.OrderDto?> getOrderByIdQueryHandler) : OrdersGrpc.OrdersGrpcBase
{
    public override async Task<GetOrderByIdResponse> GetOrderById(GetOrderByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId) || orderId == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "OrderId is invalid."));
        }

        var order = await getOrderByIdQueryHandler.Handle(new GetOrderByIdQuery(orderId), context.CancellationToken);
        if (order is null)
        {
            return new GetOrderByIdResponse { Found = false };
        }

        return new GetOrderByIdResponse
        {
            Found = true,
            Order = new ShopSystem.Contracts.Grpc.Orders.OrderDto
            {
                OrderId = order.Id.ToString(),
                UserId = order.UserId.ToString(),
                ShopId = order.ShopId.ToString(),
                ProductId = order.ProductId.ToString(),
                Quantity = order.Quantity,
                UnitPrice = decimal.ToDouble(order.UnitPrice),
                TotalPrice = decimal.ToDouble(order.TotalPrice),
                OrderedAtUtc = Timestamp.FromDateTime(DateTime.SpecifyKind(order.OrderedAtUtc, DateTimeKind.Utc)),
                Status = order.Status
            }
        };
    }
}
