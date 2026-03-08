using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Persistence;

namespace PromotionService.Application.Features.Promotions.Commands.DeletePromotion;

public sealed class DeletePromotionCommandHandler(IPromotionRepository promotionRepository) : ICommandHandler<DeletePromotionCommand, bool>
{
    public async Task<bool> Handle(DeletePromotionCommand command, CancellationToken cancellationToken)
    {
        var promotion = await promotionRepository.GetByIdAsync(command.Id, cancellationToken);
        if (promotion is null)
        {
            return false;
        }

        promotionRepository.Remove(promotion);
        await promotionRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
