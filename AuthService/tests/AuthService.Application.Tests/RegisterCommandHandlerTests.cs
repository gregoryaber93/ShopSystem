using AuthService.Application.Abstractions.Outbox;
using AuthService.Application.Abstractions.Persistence;
using AuthService.Application.Abstractions.Profiles;
using AuthService.Application.Abstractions.Security;
using AuthService.Application.Common;
using AuthService.Application.Features.Authentication.Commands.Register;
using AuthService.Contracts.Dtos;
using AuthService.Domain.Entities;
using Xunit;

namespace AuthService.Application.Tests;

public sealed class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenProfileProvisioningSucceeds_ReturnsTokenAndMarksOutboxCompleted()
    {
        var repository = new FakeAuthUserRepository();
        var outbox = new FakeAuthOutboxService();
        var profileClient = new FakeUserProfileProvisioningClient(true);
        var handler = new RegisterCommandHandler(
            outbox,
            repository,
            new FakePasswordHasherService(),
            profileClient,
            new FakeJwtTokenService());

        var command = new RegisterCommand(new RegisterRequestDto("user1@example.com", "Secret123!", ["User"]));

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("user1@example.com", result.Email);
        Assert.Equal("token", result.AccessToken);
        Assert.Single(repository.Users);
        Assert.Single(outbox.Messages);
        Assert.Equal("Completed", outbox.Messages.Single().Status);
    }

    [Fact]
    public async Task Handle_WhenProfileConflict_ThrowsConflictProfileProvisioningException()
    {
        var repository = new FakeAuthUserRepository();
        var outbox = new FakeAuthOutboxService();
        var profileClient = new FakeUserProfileProvisioningClient(false);
        var handler = new RegisterCommandHandler(
            outbox,
            repository,
            new FakePasswordHasherService(),
            profileClient,
            new FakeJwtTokenService());

        var command = new RegisterCommand(new RegisterRequestDto("user2@example.com", "Secret123!", ["User"]));

        var exception = await Assert.ThrowsAsync<ProfileProvisioningException>(() => handler.Handle(command, CancellationToken.None));

        Assert.True(exception.IsConflict);
        Assert.Single(repository.Users);
        Assert.Single(outbox.Messages);
        Assert.Equal("FailedPermanent", outbox.Messages.Single().Status);
    }

    [Fact]
    public async Task Handle_WhenProfileServiceThrows_ThrowsRetryableProfileProvisioningException()
    {
        var repository = new FakeAuthUserRepository();
        var outbox = new FakeAuthOutboxService();
        var profileClient = new ThrowingUserProfileProvisioningClient();
        var handler = new RegisterCommandHandler(
            outbox,
            repository,
            new FakePasswordHasherService(),
            profileClient,
            new FakeJwtTokenService());

        var command = new RegisterCommand(new RegisterRequestDto("user3@example.com", "Secret123!", ["User"]));

        var exception = await Assert.ThrowsAsync<ProfileProvisioningException>(() => handler.Handle(command, CancellationToken.None));

        Assert.False(exception.IsConflict);
        Assert.Single(repository.Users);
        Assert.Single(outbox.Messages);
        Assert.Equal("FailedRetryable", outbox.Messages.Single().Status);
        Assert.Equal(1, outbox.Messages.Single().RetryCount);
    }

    private sealed class FakeAuthUserRepository : IAuthUserRepository
    {
        private readonly List<AuthRoleEntity> roles =
        [
            new AuthRoleEntity { Id = Guid.NewGuid(), Name = "User" },
            new AuthRoleEntity { Id = Guid.NewGuid(), Name = "Admin" },
            new AuthRoleEntity { Id = Guid.NewGuid(), Name = "Manager" }
        ];

        public List<AuthUserEntity> Users { get; } = [];

        public Task<AuthUserEntity?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Users.FirstOrDefault(user => user.Id == userId));
        }

        public Task<AuthUserEntity?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(Users.FirstOrDefault(user => user.Email == email));
        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(Users.Any(user => user.Email == email));
        }

        public Task<IReadOnlyCollection<AuthRoleEntity>> GetOrCreateRolesAsync(IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken)
        {
            var result = roles.Where(role => roleNames.Contains(role.Name, StringComparer.OrdinalIgnoreCase)).ToArray();
            return Task.FromResult<IReadOnlyCollection<AuthRoleEntity>>(result);
        }

        public Task AddUserAsync(AuthUserEntity user, CancellationToken cancellationToken)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public void RemoveUser(AuthUserEntity user)
        {
            Users.Remove(user);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuthOutboxService : IAuthOutboxService
    {
        public List<OutboxMessageState> Messages { get; } = [];

        public Task<Guid> EnqueueUserCreatedAsync(Guid userId, string email, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            Messages.Add(new OutboxMessageState(id, "Pending", 0));
            return Task.FromResult(id);
        }

        public Task MarkCompletedAsync(Guid messageId, CancellationToken cancellationToken)
        {
            var index = Messages.FindIndex(message => message.Id == messageId);
            Messages[index] = Messages[index] with { Status = "Completed" };
            return Task.CompletedTask;
        }

        public Task MarkRetryableFailureAsync(Guid messageId, string error, DateTime nextAttemptAtUtc, CancellationToken cancellationToken)
        {
            var index = Messages.FindIndex(message => message.Id == messageId);
            Messages[index] = Messages[index] with { Status = "FailedRetryable", RetryCount = Messages[index].RetryCount + 1 };
            return Task.CompletedTask;
        }

        public Task MarkPermanentFailureAsync(Guid messageId, string error, CancellationToken cancellationToken)
        {
            var index = Messages.FindIndex(message => message.Id == messageId);
            Messages[index] = Messages[index] with { Status = "FailedPermanent" };
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserProfileProvisioningClient(bool shouldSucceed) : IUserProfileProvisioningClient
    {
        public Task<bool> CreateOrUpdateProfileAsync(Guid userId, string email, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
        {
            return Task.FromResult(shouldSucceed);
        }
    }

    private sealed class ThrowingUserProfileProvisioningClient : IUserProfileProvisioningClient
    {
        public Task<bool> CreateOrUpdateProfileAsync(Guid userId, string email, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("UserService unavailable");
        }
    }

    private sealed class FakePasswordHasherService : IPasswordHasherService
    {
        public string HashPassword(string password) => $"hash::{password}";

        public bool VerifyPassword(string password, string passwordHash) => passwordHash == $"hash::{password}";
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public (string Token, DateTime ExpiresAtUtc) CreateUserToken(Guid userId, string email, IReadOnlyCollection<string> roles)
        {
            return ("token", DateTime.UtcNow.AddHours(1));
        }
    }

    private sealed record OutboxMessageState(Guid Id, string Status, int RetryCount);
}
