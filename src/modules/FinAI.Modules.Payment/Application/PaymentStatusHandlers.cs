using FinAI.Modules.Payment.Infrastructure;

namespace FinAI.Modules.Payment.Application;

public sealed class PaymentClearedHandler(PaymentDbContext db) : IIntegrationEventHandler<PaymentClearedIntegrationEvent>
{
    public async Task HandleAsync(PaymentClearedIntegrationEvent e, CancellationToken ct)
    {
        (await db.FindPaymentAsync(e.PaymentId, ct)).Complete();
        await db.SaveChangesAsync(ct);
    }
}

public sealed class FraudFlaggedPaymentHoldHandler(PaymentDbContext db) : IIntegrationEventHandler<FraudFlaggedIntegrationEvent>
{
    public async Task HandleAsync(FraudFlaggedIntegrationEvent e, CancellationToken ct)
    {
        (await db.FindPaymentAsync(e.PaymentId, ct)).Hold();
        await db.SaveChangesAsync(ct);
    }
}

public sealed class FraudCaseResolvedPaymentHandler(PaymentDbContext db) : IIntegrationEventHandler<FraudCaseResolvedIntegrationEvent>
{
    public async Task HandleAsync(FraudCaseResolvedIntegrationEvent e, CancellationToken ct)
    {
        if (e.Resolution != FraudResolution.Approve) return;
        (await db.FindPaymentAsync(e.PaymentId, ct)).Complete();
        await db.SaveChangesAsync(ct);
    }
}

internal static class PaymentLookup
{
    public static async Task<Domain.Payment> FindPaymentAsync(this PaymentDbContext db, Guid paymentId, CancellationToken ct) =>
        await db.Payments.FindAsync([paymentId], ct)
            ?? throw new InvalidOperationException($"Payment {paymentId} not found.");
}
