namespace UserService.Application.Abstractions.Security;

public interface IPasswordHasherService
{
    string HashPassword(string password);
}
