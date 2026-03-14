using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using UserService.Application.Abstractions.Identity;

namespace UserService.Infrastructure.Identity;

internal sealed class AuthIdentityProvisioningClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor) : IAuthIdentityProvisioningClient
{
    public async Task<ProvisionedAuthIdentity?> ProvisionUserAsync(string email, string password, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/authentication/internal/provision-user")
        {
            Content = JsonContent.Create(new
            {
                Email = email,
                Password = password,
                Roles = roles
            })
        };

        ForwardAuthorization(request);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return null;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ArgumentException(message.Trim('"'));
        }

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Biezacy uzytkownik nie ma uprawnien do provisioningu identity w AuthService.");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ProvisionedAuthIdentity>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("AuthService zwrocil pusty wynik podczas provisioningu identity.");
    }

    public async Task RollbackProvisionAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/authentication/internal/users/{userId}");
        ForwardAuthorization(request);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    private void ForwardAuthorization(HttpRequestMessage request)
    {
        var authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            throw new InvalidOperationException("Brak naglowka Authorization do przekazania do AuthService.");
        }

        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
    }
}