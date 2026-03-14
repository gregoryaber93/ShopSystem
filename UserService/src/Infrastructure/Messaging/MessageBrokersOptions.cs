namespace UserService.Infrastructure.Messaging;

public sealed class MessageBrokersOptions
{
    public const string SectionName = "MessageBrokers";

    public RabbitMqOptions RabbitMq { get; set; } = new();
}

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string Username { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string Exchange { get; set; } = "AuthService.exchange";

    public string UserCreatedRoutingKey { get; set; } = "users.created.v1";

    public string UserCreatedQueue { get; set; } = "userservice.users.created.v1";
}
