using System.Diagnostics.Metrics;
using FinAI.Modules.Fraud.Domain;
using FinAI.Modules.Fraud.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinAI.Workers.Jobs;

public sealed class FraudCaseSlaJob(FraudDbContext db, ILogger<FraudCaseSlaJob> logger)
{
    private static readonly TimeSpan Sla = TimeSpan.FromMinutes(15);
    private static readonly Meter Meter = new("FinAI.Fraud");
    private static readonly Counter<long> Breaches =
        Meter.CreateCounter<long>("finai.fraud.sla_breaches", description: "Open fraud cases older than the pickup SLA, per check");

    public async Task RunAsync(CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow - Sla;
        var overdue = await db.Cases.AsNoTracking()
            .Where(c => c.Status == FraudCaseStatus.Open && c.CreatedOnUtc < cutoff)
            .OrderBy(c => c.CreatedOnUtc)
            .Select(c => new { c.PaymentId, c.CreatedOnUtc })
            .ToListAsync(ct);

        if (overdue.Count == 0) return;

        Breaches.Add(overdue.Count);
        logger.LogWarning(
            "{Count} fraud case(s) open longer than {SlaMinutes} min; oldest payment {PaymentId} opened {OpenedOnUtc:u}",
            overdue.Count, Sla.TotalMinutes, overdue[0].PaymentId, overdue[0].CreatedOnUtc);
    }
}
