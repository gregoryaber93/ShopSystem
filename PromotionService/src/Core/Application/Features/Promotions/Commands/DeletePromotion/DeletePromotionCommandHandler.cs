using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Application.Abstractions.Security;

namespace PromotionService.Application.Features.Promotions.Commands.DeletePromotion;

public sealed class DeletePromotionCommandHandler(
    IPromotionRepository promotionRepository,
    ICurrentUserService currentUserService) : ICommandHandler<DeletePromotionCommand, bool>
{
    public async Task<bool> Handle(DeletePromotionCommand command, CancellationToken cancellationToken)
    {
        var promotion = await promotionRepository.GetByIdAsync(command.Id, cancellationToken);
        if (promotion is null)
        {
            return false;
        }

        if (currentUserService.IsInRole("Manager"))
        {
            var currentUserId = currentUserService.GetUserIdOrThrow();
            if (promotion.CreatedByUserId != currentUserId)
            {
                throw new UnauthorizedAccessException("Manager can only delete their own promotions.");
            }
        }

        promotionRepository.Remove(promotion);
        await promotionRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
