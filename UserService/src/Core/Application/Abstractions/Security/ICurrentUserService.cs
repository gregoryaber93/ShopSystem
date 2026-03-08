namespace UserService.Application.Abstractions.Security;

public interface ICurrentUserService
{
    bool IsInRole(string role);
}
