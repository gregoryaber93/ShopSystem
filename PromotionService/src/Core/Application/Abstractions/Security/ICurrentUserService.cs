namespace PromotionService.Application.Abstractions.Security;

public interface ICurrentUserService
{
    bool IsInRole(string role);
    Guid GetUserIdOrThrow();
}
