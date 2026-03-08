namespace ShopService.Application.Abstractions.Messaging;

public interface IGrpcServiceClient
{
    string ServiceName { get; }
    Uri Address { get; }
}