using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Context.Propagation;

namespace FinAI.BuildingBlocks.Infrastructure.Messaging;

public sealed class KafkaConsumerBackgroundService<T, THandler>(IConfiguration cfg, IServiceScopeFactory scopes, ILogger<KafkaConsumerBackgroundService<T, THandler>> log): BackgroundService
    where T : IntegrationEvent
    where THandler : IIntegrationEventHandler<T>
{
    private static readonly ActivitySource Activity = new("FinAI.Kafka");
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)];
    private static readonly TimeSpan DeadLetterRetryDelay = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var topic = TopicResolver.For<T>();
        var groupId = $"{topic}.{typeof(THandler).Name}";
        var bootstrap = cfg["Kafka:BootstrapServers"];

        using var consumer = new ConsumerBuilder<string, byte[]>(new ConsumerConfig
        {
            BootstrapServers = bootstrap,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        }).Build();
        using var deadLetters = new ProducerBuilder<string, byte[]>(new ProducerConfig
        {
            BootstrapServers = bootstrap,
            Acks = Acks.All,
            EnableIdempotence = true
        }).Build();

        consumer.Subscribe(topic);

        while (!ct.IsCancellationRequested)
        {
            ConsumeResult<string, byte[]> result;
            try { result = consumer.Consume(ct); }
            catch (OperationCanceledException) { break; }
            catch (ConsumeException ex)
            {
                log.LogWarning(ex, "Consume from {Topic} failed; retrying.", topic);
                continue;
            }

            var parent = Propagator.Extract(default, result.Message.Headers, (h, k) => h.TryGetLastBytes(k, out var v) ? [Encoding.UTF8.GetString(v)] : []);
            using var span = Activity.StartActivity($"consume {topic}", ActivityKind.Consumer, parent.ActivityContext);

            Exception? failure;
            try { failure = await HandleWithRetriesAsync(result, ct); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }

            if (failure is null)
            {
                consumer.Commit(result);
                continue;
            }

            span?.SetStatus(ActivityStatusCode.Error, failure.Message);
            try
            {
                await deadLetters.ProduceAsync($"{topic}.dlt", ToDeadLetter(result, groupId, failure), ct);
                consumer.Commit(result);
                log.LogError(failure, "Handler {Handler} failed for {Topic} at {Offset}; moved to {Topic}.dlt.",
                    typeof(THandler).Name, topic, result.TopicPartitionOffset, topic);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
            catch (Exception dltEx)
            {
                log.LogError(dltEx, "Could not dead-letter {Topic} at {Offset}; will reprocess.", topic, result.TopicPartitionOffset);
                consumer.Seek(result.TopicPartitionOffset);
                await Task.Delay(DeadLetterRetryDelay, ct);
            }
        }

        consumer.Close();
    }

    private async Task<Exception?> HandleWithRetriesAsync(ConsumeResult<string, byte[]> result, CancellationToken ct)
    {
        T @event;
        try
        {
            @event = JsonSerializer.Deserialize<T>(result.Message.Value)
                ?? throw new JsonException("Message deserialized to null.");
        }
        catch (JsonException ex)
        {
            return ex;
        }

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<THandler>().HandleAsync(@event, ct);
                return null;
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                if (attempt >= RetryDelays.Length) return ex;
                log.LogWarning(ex, "Handler {Handler} failed (attempt {Attempt}); retrying in {Delay}.",
                    typeof(THandler).Name, attempt + 1, RetryDelays[attempt]);
                await Task.Delay(RetryDelays[attempt], ct);
            }
        }
    }

    private static Message<string, byte[]> ToDeadLetter(ConsumeResult<string, byte[]> result, string groupId, Exception failure)
    {
        var headers = new Headers();
        foreach (var h in result.Message.Headers) headers.Add(h.Key, h.GetValueBytes());
        headers.Add("dlt-original-topic", Encoding.UTF8.GetBytes(result.Topic));
        headers.Add("dlt-original-partition", Encoding.UTF8.GetBytes(result.Partition.Value.ToString()));
        headers.Add("dlt-original-offset", Encoding.UTF8.GetBytes(result.Offset.Value.ToString()));
        headers.Add("dlt-consumer-group", Encoding.UTF8.GetBytes(groupId));
        headers.Add("dlt-error", Encoding.UTF8.GetBytes($"{failure.GetType().Name}: {failure.Message}"));
        headers.Add("dlt-failed-at", Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")));
        return new Message<string, byte[]> { Key = result.Message.Key, Value = result.Message.Value, Headers = headers };
    }
}
