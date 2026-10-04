using FinAI.BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinAI.Workers.Jobs;

public sealed class OutboxCleanupJob<TContext>(TContext db, ILogger<OutboxCleanupJob<TContext>> logger)
    where TContext : DbContext, IHasOutbox
{
    private static readonly TimeSpan Retention = TimeSpan.FromDays(7);

    public async Task RunAsync(CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow - Retention;
        var deleted = await db.Outbox
            .Where(m => m.ProcessedOnUtc != null && m.ProcessedOnUtc < cutoff)
            .ExecuteDeleteAsync(ct);

        logger.LogInformation("Outbox cleanup ({Context}): deleted {Deleted} published rows older than {Days} days",
            typeof(TContext).Name, deleted, Retention.TotalDays);
    }
}
