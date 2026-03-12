namespace OrderService.Infrastructure.Security;

public interface IJwtTokenService
{
    string CreateServiceToken(string subject);
}
