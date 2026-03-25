using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopService.Application.Abstractions.CQRS;
using ShopService.Application.Features.Shops.Commands.AddShop;
using ShopService.Application.Features.Shops.Commands.DeleteShop;
using ShopService.Application.Features.Shops.Commands.UpdateShop;
using ShopService.Application.Features.Shops.Queries.GetShops;
using ShopService.Contracts.Dtos;

namespace ShopService.Api.Controllers;

[ApiController]
[Route("api/shop")]
[Authorize(Roles = "Admin,Manager,User")]
public class ShopsController(
    IQueryHandler<GetShopsQuery, IReadOnlyCollection<ShopDto>> getShopsQueryHandler,
    ICommandHandler<AddShopCommand, ShopDto> addShopCommandHandler,
    ICommandHandler<UpdateShopCommand, ShopDto?> updateShopCommandHandler,
    ICommandHandler<DeleteShopCommand, bool> deleteShopCommandHandler) : ControllerBase
{
    [HttpGet("getShops")]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<IReadOnlyCollection<ShopDto>>> GetShops(CancellationToken cancellationToken)
    {
        var shops = await getShopsQueryHandler.Handle(new GetShopsQuery(), cancellationToken);

        return Ok(shops);
    }

    [HttpPost("addShop")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ShopDto>> AddShop([FromBody] ShopDto shopDto, CancellationToken cancellationToken)
    {
        var createdShop = await addShopCommandHandler.Handle(new AddShopCommand(shopDto), cancellationToken);
        return Created($"/api/shops/{createdShop.Id}", createdShop);
    }

    [HttpPut("updateShop/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ShopDto>> UpdateShop(Guid id, [FromBody] ShopDto shopDto, CancellationToken cancellationToken)
    {
        try
        {
            var updatedShop = await updateShopCommandHandler.Handle(new UpdateShopCommand(id, shopDto), cancellationToken);
            if (updatedShop is null)
            {
                return NotFound();
            }

            return Ok(updatedShop);
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
        }
    }

    [HttpDelete("deleteShop/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> DeleteShop(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await deleteShopCommandHandler.Handle(new DeleteShopCommand(id), cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
        }
    }
}
