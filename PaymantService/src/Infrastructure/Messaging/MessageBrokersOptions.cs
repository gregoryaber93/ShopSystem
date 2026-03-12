namespace PaymantService.Infrastructure.Messaging;

public sealed class MessageBrokersOptions
{
    public const string SectionName = "MessageBrokers";

    public RabbitMqOptions RabbitMq { get; set; } = new();

    public KafkaOptions Kafka { get; set; } = new();
}

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string Username { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string Exchange { get; set; } = "PaymantService.exchange";

    public string DeadLetterExchange { get; set; } = "PaymantService.exchange.dlq";
}

public sealed class KafkaOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";

    public string TopicPrefix { get; set; } = "payments";
}
