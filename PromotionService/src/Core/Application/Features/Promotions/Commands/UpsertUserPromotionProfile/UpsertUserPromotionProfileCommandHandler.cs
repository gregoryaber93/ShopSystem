using System.Text.Json;
using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Application.Features.Promotions.EventSourcing;
using PromotionService.Contracts.Dtos;
using PromotionService.Contracts.Messaging;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Commands.UpsertUserPromotionProfile;

public sealed class UpsertUserPromotionProfileCommandHandler(
    IUserPromotionProfileRepository userPromotionProfileRepository,
    ILoyaltyEventStore loyaltyEventStore) : ICommandHandler<UpsertUserPromotionProfileCommand, UserPromotionProfileDto>
{
    public async Task<UserPromotionProfileDto> Handle(UpsertUserPromotionProfileCommand command, CancellationToken cancellationToken)
    {
        Validate(command.Profile);

        var existingProfile = await userPromotionProfileRepository.GetByUserIdAsync(command.Profile.UserId, cancellationToken);
        var pointsDelta = command.Profile.LoyaltyPoints - (existingProfile?.LoyaltyPoints ?? 0m);

        var profile = new UserPromotionProfileEntity
        {
            UserId = command.Profile.UserId,
            LoyaltyPoints = command.Profile.LoyaltyPoints,
            OrdersCount = command.Profile.OrdersCount,
            TotalSpent = command.Profile.TotalSpent,
            LastOrderAtUtc = command.Profile.LastOrderAtUtc
        };

        var occurredOnUtc = DateTime.UtcNow;
        var expectedVersion = await loyaltyEventStore.GetLatestVersionAsync(command.Profile.UserId, cancellationToken);

        if (pointsDelta > 0)
        {
            var pointsEarned = new PointsEarnedEventV1(command.Profile.UserId, pointsDelta, occurredOnUtc);
            await loyaltyEventStore.AppendAsync(
                command.Profile.UserId,
                "PointsEarned",
                1,
                JsonSerializer.Serialize(pointsEarned),
                occurredOnUtc,
                expectedVersion,
                cancellationToken);

            expectedVersion += 1;
        }
        else if (pointsDelta < 0)
        {
            var pointsSpent = new PointsSpentEventV1(command.Profile.UserId, decimal.Abs(pointsDelta), occurredOnUtc);
            await loyaltyEventStore.AppendAsync(
                command.Profile.UserId,
                "PointsSpent",
                1,
                JsonSerializer.Serialize(pointsSpent),
                occurredOnUtc,
                expectedVersion,
                cancellationToken);

            expectedVersion += 1;
        }

        var profileUpdated = new LoyaltyProfileUpdatedEventV1(
            command.Profile.UserId,
            command.Profile.LoyaltyPoints,
            command.Profile.OrdersCount,
            command.Profile.TotalSpent,
            command.Profile.LastOrderAtUtc,
            occurredOnUtc);

        await loyaltyEventStore.AppendAsync(
            command.Profile.UserId,
            "LoyaltyProfileUpdated",
            1,
            JsonSerializer.Serialize(profileUpdated),
            occurredOnUtc,
            expectedVersion,
            cancellationToken);

        expectedVersion += 1;

        var aggregate = new LoyaltyAggregateState();
        aggregate.Apply(profileUpdated, expectedVersion);
        await loyaltyEventStore.SaveSnapshotAsync(new LoyaltySnapshotEntity
        {
            AggregateId = command.Profile.UserId,
            Version = aggregate.Version,
            Payload = JsonSerializer.Serialize(aggregate.ToSnapshot()),
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

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
