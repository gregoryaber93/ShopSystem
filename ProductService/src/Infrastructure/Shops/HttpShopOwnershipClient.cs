using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using ProductService.Application.Abstractions.Shops;

namespace ProductService.Infrastructure.Shops;

internal sealed class HttpShopOwnershipClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor) : IShopOwnershipClient
{
    private record ShopOwnerResponse(Guid? OwnerUserId);

    public async Task<Guid?> GetShopOwnerAsync(Guid shopId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/shops/internal/{shopId}/owner");
        ForwardAuthorization(request);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ShopOwnerResponse>(cancellationToken: cancellationToken);
        return result?.OwnerUserId;
    }

    private void ForwardAuthorization(HttpRequestMessage request)
    {
        var authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        }
    }
}
