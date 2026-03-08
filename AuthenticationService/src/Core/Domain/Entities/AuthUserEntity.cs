namespace AuthenticationService.Domain.Entities;

public sealed class AuthUserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<AuthUserRoleEntity> UserRoles { get; set; } = [];
}
