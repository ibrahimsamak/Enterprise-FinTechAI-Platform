using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace FinAI.BuildingBlocks.Infrastructure;

public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Topic { get; set; } = default!;
    public string ClrType { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public string? TraceParent { get; set; }
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOnUtc { get; set; }
}

public interface IHasOutbox
{
    DbSet<OutboxMessage> Outbox { get; }
}

public static class OutboxExtensions
{
    public static void Enqueue<T>(this DbSet<OutboxMessage> outbox, T @event) where T : IntegrationEvent
    {
        outbox.Add(new OutboxMessage
        {
            Topic = TopicResolver.For<T>(),
            ClrType = typeof(T).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(@event, @event.GetType()),
            TraceParent = Activity.Current?.Id,
            OccurredOnUtc = @event.OccurredOn
        });
    }
}