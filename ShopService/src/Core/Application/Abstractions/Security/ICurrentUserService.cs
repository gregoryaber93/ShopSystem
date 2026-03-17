namespace ShopService.Application.Abstractions.Security;

public interface ICurrentUserService
{
    bool IsInRole(string role);
    Guid GetUserIdOrThrow();
}
