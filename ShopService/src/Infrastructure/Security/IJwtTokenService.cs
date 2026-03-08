namespace ShopService.Infrastructure.Security;

public interface IJwtTokenService
{
    string CreateServiceToken(string subject);
}
