using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ProductService.Application.Abstractions.Security;

namespace ProductService.Infrastructure.Security;

internal sealed class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public bool IsInRole(string role)
    {
        var user = httpContextAccessor.HttpContext?.User;
        return user?.IsInRole(role) ?? false;
    }

    public Guid GetUserIdOrThrow()
    {
        var user = httpContextAccessor.HttpContext?.User;
        var sub = user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("User identity not found.");
        }

        return userId;
    }
}
