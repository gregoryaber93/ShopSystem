namespace AuthenticationService.Domain.Entities;

public sealed class AuthRoleEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<AuthUserRoleEntity> UserRoles { get; set; } = [];
}
