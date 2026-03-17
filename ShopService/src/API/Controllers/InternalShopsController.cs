using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopService.Application.Abstractions.Persistence;

namespace ShopService.Api.Controllers;

[ApiController]
[Route("api/shops/internal")]
[Authorize(Roles = "Admin,Manager")]
public class InternalShopsController(IShopRepository shopRepository) : ControllerBase
{
    [HttpGet("{shopId:guid}/owner")]
    public async Task<ActionResult> GetShopOwner(Guid shopId, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetByIdAsync(shopId, cancellationToken);
        if (shop is null)
        {
            return NotFound();
        }

        return Ok(new { OwnerUserId = shop.OwnerUserId });
    }
}
