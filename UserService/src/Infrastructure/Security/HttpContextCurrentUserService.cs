using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UserService.Application.Abstractions.Security;

namespace UserService.Infrastructure.Security;

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
}
