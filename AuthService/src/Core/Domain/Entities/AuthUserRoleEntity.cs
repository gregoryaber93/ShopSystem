namespace AuthService.Domain.Entities;

public sealed class AuthUserRoleEntity
{
    public Guid UserId { get; set; }
    public AuthUserEntity User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public AuthRoleEntity Role { get; set; } = null!;
}
