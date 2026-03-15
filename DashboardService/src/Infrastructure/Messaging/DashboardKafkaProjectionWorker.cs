using Confluent.Kafka;
using DashboardService.Application.Abstractions;
using DashboardService.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace DashboardService.Infrastructure.Messaging;

public sealed class DashboardKafkaProjectionWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaConsumerOptions> options,
    ILogger<DashboardKafkaProjectionWorker> logger) : BackgroundService
{
    private readonly KafkaConsumerOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topics = _options.Topics.Where(topic => !string.IsNullOrWhiteSpace(topic)).Distinct().ToArray();
        if (topics.Length == 0)
        {
            logger.LogWarning("Dashboard Kafka worker disabled because no topics were configured.");
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

        logger.LogInformation("Dashboard Kafka worker subscribed to {TopicCount} topics using group {GroupId}.", topics.Length, _options.GroupId);

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
                var store = scope.ServiceProvider.GetRequiredService<IDashboardStore>();

                var eventType = ReadHeader(result.Message.Headers, "eventType") ?? "unknown";
                var correlationId = ReadHeader(result.Message.Headers, "correlationId");
                var occurredOnUtc = ParseUtc(ReadHeader(result.Message.Headers, "occurredOnUtc")) ?? DateTime.UtcNow;

                var item = new DashboardItem
                {
                    Id = Guid.NewGuid(),
                    Level = "Information",
                    Message = $"projection={ResolveProjection(result.Topic)} event={eventType} key={result.Message.Key ?? ""} payload={result.Message.Value}",
                    Source = $"kafka:{result.Topic}",
                    CorrelationId = correlationId,
                    CreatedAtUtc = occurredOnUtc
                };

                await store.AddAsync(item, stoppingToken);
                consumer.StoreOffset(result);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                logger.LogError(ex, "Dashboard Kafka consume error on topic {Topic}.", ex.ConsumerRecord?.Topic);
                await Task.Delay(500, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dashboard Kafka projection failed.");
                await Task.Delay(500, stoppingToken);
            }
        }

        consumer.Close();
    }

    private static string ResolveProjection(string topic)
    {
        if (topic.Contains("order", StringComparison.OrdinalIgnoreCase))
        {
            return "orders_projection";
        }

        if (topic.Contains("payment", StringComparison.OrdinalIgnoreCase))
        {
            return "payments_projection";
        }

        return "loyalty_projection";
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
