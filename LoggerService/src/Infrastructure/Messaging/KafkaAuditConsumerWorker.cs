using Confluent.Kafka;
using LoggerService.Application.Abstractions;
using LoggerService.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace LoggerService.Infrastructure.Messaging;

public sealed class KafkaAuditConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaConsumerOptions> options,
    ILogger<KafkaAuditConsumerWorker> logger) : BackgroundService
{
    private readonly KafkaConsumerOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topics = _options.Topics.Where(topic => !string.IsNullOrWhiteSpace(topic)).Distinct().ToArray();
        if (topics.Length == 0)
        {
            logger.LogWarning("Logger Kafka audit consumer disabled because no topics were configured.");
            return;
        }

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topics);

        logger.LogInformation("Logger Kafka worker subscribed to {TopicCount} topics using group {GroupId}.", topics.Length, _options.GroupId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result is null)
                {
                    continue;
                }

                using var scope = scopeFactory.CreateScope();
                var logStore = scope.ServiceProvider.GetRequiredService<ILogStore>();

                var eventType = ReadHeader(result.Message.Headers, "eventType") ?? "unknown";
                var eventVersion = ReadHeader(result.Message.Headers, "eventVersion") ?? "1";
                var correlationId = ReadHeader(result.Message.Headers, "correlationId");
                var createdAtUtc = ParseUtc(ReadHeader(result.Message.Headers, "occurredOnUtc")) ?? DateTime.UtcNow;

                var entry = new LogEntry
                {
                    Id = Guid.NewGuid(),
                    Level = "Information",
                    Message = $"KafkaAudit topic={result.Topic} eventType={eventType} eventVersion={eventVersion} key={result.Message.Key ?? ""} payload={result.Message.Value}",
                    Source = "KafkaAuditConsumer",
                    CorrelationId = correlationId,
                    CreatedAtUtc = createdAtUtc
                };

                await logStore.AddAsync(entry, stoppingToken);
                consumer.StoreOffset(result);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                logger.LogError(ex, "Logger Kafka consume error on topic {Topic}.", ex.ConsumerRecord?.Topic);
                await Task.Delay(500, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Logger Kafka audit handling failed.");
                await Task.Delay(500, stoppingToken);
            }
        }

        consumer.Close();
    }

    private static string? ReadHeader(Headers? headers, string key)
    {
        if (headers is null)
        {
            return null;
        }

        for (var i = headers.Count - 1; i >= 0; i--)
        {
            var header = headers[i];
            if (!string.Equals(header.Key, key, StringComparison.OrdinalIgnoreCase) || header.GetValueBytes() is null)
            {
                continue;
            }

            return System.Text.Encoding.UTF8.GetString(header.GetValueBytes()!);
        }

        return null;
    }

    private static DateTime? ParseUtc(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed.ToUniversalTime()
            : null;
    }
}
