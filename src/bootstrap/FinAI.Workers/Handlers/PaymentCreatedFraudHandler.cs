using FinAI.AI.Abstractions.Fraud;
using FinAI.BuildingBlocks.Infrastructure;
using FinAI.Modules.Fraud;
using FinAI.Modules.Fraud.Domain;
using FinAI.Modules.Fraud.Infrastructure;
using Microsoft.EntityFrameworkCore;

public sealed class PaymentCreatedFraudHandler(
    FraudRulesEngine engine, IFraudAgent agent, FraudDbContext db, ILogger<PaymentCreatedFraudHandler> log
): IIntegrationEventHandler<PaymentCreatedIntegrationEvent>
{
    public async Task HandleAsync(PaymentCreatedIntegrationEvent e, CancellationToken ct)
    {
        var decision = engine.Evaluate(e);
        if (!decision.IsHighRisk)
        {
            db.Outbox.Enqueue(new PaymentClearedIntegrationEvent(e.PaymentId, decision.Score));
            await db.SaveChangesAsync(ct);
            return;
        }

        if (await db.Cases.AnyAsync(c => c.PaymentId == e.PaymentId, ct)) return;

        string investigation;
        try
        {
            investigation = await agent.InvestigateAsync(e, decision, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "AI investigation failed for payment {PaymentId}; opening case for manual review.", e.PaymentId);
            investigation = "AI investigation unavailable - manual review required. Rule flags: " + string.Join("; ", decision.Reasons);
        }
        db.Cases.Add(FraudCase.Open(e, decision, investigation));
        db.Outbox.Enqueue(new FraudFlaggedIntegrationEvent(             
            e.PaymentId, e.CustomerId, e.Amount, e.Currency, e.Country,
            decision.Score, decision.Reasons.ToArray(), investigation));

        await db.SaveChangesAsync(ct);
    }
}
