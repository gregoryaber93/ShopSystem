using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ShopService.Application.Abstractions.Security;

namespace ShopService.Infrastructure.Security;

internal sealed class HttpContextCurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public bool IsInRole(string role)
    {
        var user = accessor.HttpContext?.User;
        if (user is null)
        {
            return false;
        }

        return user.Claims.Any(claim =>
            claim.Type == ClaimTypes.Role &&
            string.Equals(claim.Value, role, StringComparison.OrdinalIgnoreCase));
    }

    public Guid GetUserIdOrThrow()
    {
        var principal = accessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("Brak kontekstu uzytkownika.");

        var subValue = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(subValue, out var userId) || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Nie mozna odczytac UserId z tokenu.");
        }

        return userId;
    }
}
