using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace FinAI.BuildingBlocks.Infrastructure.Messaging;

public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private static readonly ActivitySource Source = new("FinAI.Kafka");
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
    private readonly IProducer<string, byte[]> _producer;

    public KafkaEventPublisher(IConfiguration cfg) =>
        _producer = KafkaClients.Producer(cfg);

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent
    {
        var topic = TopicResolver.For<T>();
        using var span = Source.StartActivity($"publish {topic}", ActivityKind.Producer);
        span?.SetTag("messaging.system", "kafka");
        span?.SetTag("messaging.destination.name", topic);

        var headers = new Headers { { "ce-type", Encoding.UTF8.GetBytes(typeof(T).AssemblyQualifiedName!) } };
        var context = (span ?? Activity.Current)?.Context ?? default;
        Propagator.Inject(new PropagationContext(context, Baggage.Current), headers,
            (h, k, v) => h.Add(k, Encoding.UTF8.GetBytes(v)));

        try
        {
            await _producer.ProduceAsync(topic, new Message<string, byte[]>
            {
                Key = @event.PartitionKey,
                Value = JsonSerializer.SerializeToUtf8Bytes(@event, typeof(T)),
                Headers = headers
            }, ct);
        }
        catch (ProduceException<string, byte[]> ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Error.Reason);
            throw;
        }
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
