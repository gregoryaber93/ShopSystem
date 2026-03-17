using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Features.Promotions.Commands.AddPromotion;
using PromotionService.Application.Features.Promotions.Commands.DeletePromotion;
using PromotionService.Application.Features.Promotions.Commands.UpsertUserPromotionProfile;
using PromotionService.Application.Features.Promotions.Commands.UpdatePromotion;
using PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions;
using PromotionService.Application.Features.Promotions.Queries.GetPromotions;
using PromotionService.Application.Features.Promotions.Queries.ReplayUserLoyaltyProjection;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,User")]
public class PromotionsController(
    IQueryHandler<GetPromotionsQuery, IReadOnlyCollection<PromotionDto>> getPromotionsQueryHandler,
    IQueryHandler<EvaluatePromotionsQuery, PromotionEvaluationResultDto> evaluatePromotionsQueryHandler,
    IQueryHandler<ReplayUserLoyaltyProjectionQuery, UserPromotionProfileDto?> replayUserLoyaltyProjectionQueryHandler,
    ICommandHandler<AddPromotionCommand, PromotionDto> addPromotionCommandHandler,
    ICommandHandler<UpdatePromotionCommand, PromotionDto?> updatePromotionCommandHandler,
    ICommandHandler<UpsertUserPromotionProfileCommand, UserPromotionProfileDto> upsertUserPromotionProfileCommandHandler,
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

    [HttpPost("evaluate")]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<PromotionEvaluationResultDto>> EvaluatePromotions(
        [FromBody] PromotionEvaluationRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var evaluation = await evaluatePromotionsQueryHandler.Handle(new EvaluatePromotionsQuery(request), cancellationToken);
            return Ok(evaluation);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("user-profile")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<UserPromotionProfileDto>> UpsertUserProfile(
        [FromBody] UserPromotionProfileDto profile,
        CancellationToken cancellationToken)
    {
        try
        {
            var upserted = await upsertUserPromotionProfileCommandHandler
                .Handle(new UpsertUserPromotionProfileCommand(profile), cancellationToken);

            return Ok(upserted);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPost("replay-user-profile/{userId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<UserPromotionProfileDto>> ReplayUserProfile(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var rebuilt = await replayUserLoyaltyProjectionQueryHandler
                .Handle(new ReplayUserLoyaltyProjectionQuery(userId), cancellationToken);

            if (rebuilt is null)
            {
                return NotFound();
            }

            return Ok(rebuilt);
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
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = exception.Message });
        }
    }

    [HttpDelete("deletePromotion/{id:guid}")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult> DeletePromotion(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await deletePromotionCommandHandler.Handle(new DeletePromotionCommand(id), cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = exception.Message });
        }
    }
}
