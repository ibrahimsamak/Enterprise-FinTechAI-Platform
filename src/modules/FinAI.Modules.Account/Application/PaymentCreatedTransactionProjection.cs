using FinAI.Modules.Account.Infrastructure;
using FinAI.Modules.Account.Infrastructure.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace FinAI.Modules.Account.Application;

public sealed class PaymentCreatedTransactionProjection(AccountDbContext db)
    : IIntegrationEventHandler<PaymentCreatedIntegrationEvent>
{
    public async Task HandleAsync(PaymentCreatedIntegrationEvent e, CancellationToken ct)
    {
        if (await db.Transactions.AnyAsync(t => t.PaymentId == e.PaymentId, ct)) return;

        db.Transactions.Add(new CustomerTransaction
        {
            PaymentId = e.PaymentId,
            CustomerId = e.CustomerId,
            AccountId = e.AccountId,
            Amount = e.Amount,
            Currency = e.Currency,
            Country = e.Country,
            OccurredOnUtc = e.OccurredOn
        });
        await db.SaveChangesAsync(ct);
    }
}
