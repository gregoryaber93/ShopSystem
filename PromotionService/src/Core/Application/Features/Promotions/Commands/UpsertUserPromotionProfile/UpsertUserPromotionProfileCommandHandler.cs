using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Commands.UpsertUserPromotionProfile;

public sealed class UpsertUserPromotionProfileCommandHandler(
    IUserPromotionProfileRepository userPromotionProfileRepository) : ICommandHandler<UpsertUserPromotionProfileCommand, UserPromotionProfileDto>
{
    public async Task<UserPromotionProfileDto> Handle(UpsertUserPromotionProfileCommand command, CancellationToken cancellationToken)
    {
        Validate(command.Profile);

        var profile = new UserPromotionProfileEntity
        {
            UserId = command.Profile.UserId,
            LoyaltyPoints = command.Profile.LoyaltyPoints,
            OrdersCount = command.Profile.OrdersCount,
            TotalSpent = command.Profile.TotalSpent,
            LastOrderAtUtc = command.Profile.LastOrderAtUtc
        };

        await userPromotionProfileRepository.UpsertAsync(profile, cancellationToken);
        await userPromotionProfileRepository.SaveChangesAsync(cancellationToken);

        return command.Profile;
    }

    private static void Validate(UserPromotionProfileDto profile)
    {
        if (profile.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.");
        }

        if (profile.LoyaltyPoints < 0)
        {
            throw new ArgumentException("LoyaltyPoints cannot be negative.");
        }

        if (profile.OrdersCount < 0)
        {
            throw new ArgumentException("OrdersCount cannot be negative.");
        }

        if (profile.TotalSpent < 0)
        {
            throw new ArgumentException("TotalSpent cannot be negative.");
        }
    }
}
