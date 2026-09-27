using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinAI.BuildingBlocks.Infrastructure.Messaging;

public sealed class OutboxDispatcher<TDbContext>(
    IServiceScopeFactory scopes, IEventPublisher publisher, ILogger<OutboxDispatcher<TDbContext>> log)
    : BackgroundService where TDbContext : DbContext, IHasOutbox
{
    private static readonly MethodInfo PublishMethod =
        typeof(IEventPublisher).GetMethod(nameof(IEventPublisher.PublishAsync))!;
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(500);
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(ct);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                log.LogError(ex, "Outbox dispatch for {Context} failed; retrying in {Interval}",
                    typeof(TDbContext).Name, PollInterval);
            }
            await Task.Delay(PollInterval, ct);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var batch = await db.Outbox.Where(o => o.ProcessedOnUtc == null)
                                   .OrderBy(o => o.OccurredOnUtc).Take(BatchSize).ToListAsync(ct);
        if (batch.Count == 0) return;

        try
        {
            foreach (var msg in batch)
            {
                var type = Type.GetType(msg.ClrType)
                    ?? throw new InvalidOperationException($"Outbox message {msg.Id}: unknown type '{msg.ClrType}'.");
                var @event = JsonSerializer.Deserialize(msg.Payload, type)
                    ?? throw new InvalidOperationException($"Outbox message {msg.Id}: payload deserialized to null.");
                await (Task)PublishMethod.MakeGenericMethod(type).Invoke(publisher, [@event, ct])!;
                msg.ProcessedOnUtc = DateTime.UtcNow;
            }
        }
        finally
        {
            await db.SaveChangesAsync(CancellationToken.None);
        }
    }
}
