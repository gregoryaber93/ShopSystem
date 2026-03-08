using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Features.Promotions.Commands.AddPromotion;
using PromotionService.Application.Features.Promotions.Commands.DeletePromotion;
using PromotionService.Application.Features.Promotions.Commands.UpdatePromotion;
using PromotionService.Application.Features.Promotions.Queries.GetPromotions;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,User")]
public class PromotionsController(
    IQueryHandler<GetPromotionsQuery, IReadOnlyCollection<PromotionDto>> getPromotionsQueryHandler,
    ICommandHandler<AddPromotionCommand, PromotionDto> addPromotionCommandHandler,
    ICommandHandler<UpdatePromotionCommand, PromotionDto?> updatePromotionCommandHandler,
    ICommandHandler<DeletePromotionCommand, bool> deletePromotionCommandHandler) : ControllerBase
{
    [HttpGet("getPromotions")]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<IReadOnlyCollection<PromotionDto>>> GetPromotions(CancellationToken cancellationToken)
    {
        var promotions = await getPromotionsQueryHandler.Handle(new GetPromotionsQuery(), cancellationToken);
        return Ok(promotions);
    }

    [HttpPost("addPromotion")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<PromotionDto>> AddPromotion([FromBody] PromotionDto promotionDto, CancellationToken cancellationToken)
    {
        try
        {
            var createdPromotion = await addPromotionCommandHandler.Handle(new AddPromotionCommand(promotionDto), cancellationToken);
            return Created($"/api/promotions/{createdPromotion.Id}", createdPromotion);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("updatePromotion/{id:guid}")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<PromotionDto>> UpdatePromotion(Guid id, [FromBody] PromotionDto promotionDto, CancellationToken cancellationToken)
    {
        try
        {
            var updatedPromotion = await updatePromotionCommandHandler.Handle(new UpdatePromotionCommand(id, promotionDto), cancellationToken);
            if (updatedPromotion is null)
            {
                return NotFound();
            }

            return Ok(updatedPromotion);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpDelete("deletePromotion/{id:guid}")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult> DeletePromotion(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await deletePromotionCommandHandler.Handle(new DeletePromotionCommand(id), cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
