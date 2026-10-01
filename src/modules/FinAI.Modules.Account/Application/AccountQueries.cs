using FinAI.AI.Abstractions.Tools;
using FinAI.BuildingBlocks.Application;
using FinAI.Modules.Account.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinAI.Modules.Account.Application;

public sealed class AccountQueries(AccountDbContext db) : IAccountQueries
{
    private static readonly TimeSpan HistoryWindow = TimeSpan.FromDays(90);   
    private const int MaxTransactions = 50;

    public async Task<IReadOnlyList<TxnDto>> GetRecentTransactions(
        Guid customerId, ICurrentUser caller, Guid? excludePaymentId = null, CancellationToken ct = default)
    {
        EnsureCanRead(customerId, caller);
        return await History(customerId, excludePaymentId)
            .OrderByDescending(t => t.OccurredOnUtc)
            .Take(MaxTransactions)
            .Select(t => new TxnDto(t.PaymentId, t.Amount, t.Currency, t.Country, t.OccurredOnUtc))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetLocations(
        Guid customerId, ICurrentUser caller, Guid? excludePaymentId = null, CancellationToken ct = default)
    {
        EnsureCanRead(customerId, caller);
        return await History(customerId, excludePaymentId)
            .GroupBy(t => t.Country)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .ToListAsync(ct);
    }

    private IQueryable<Infrastructure.ReadModels.CustomerTransaction> History(Guid customerId, Guid? excludePaymentId)
    {
        var since = DateTime.UtcNow - HistoryWindow;
        return db.Transactions.AsNoTracking()
            .Where(t => t.CustomerId == customerId && t.OccurredOnUtc >= since && t.PaymentId != excludePaymentId);
    }

    private static void EnsureCanRead(Guid customerId, ICurrentUser caller)
    {
        var allowed = caller.Role is "FraudOfficer" or "FraudSystem" || caller.CustomerId == customerId;
        if (!allowed) throw new UnauthorizedAccessException("Caller may not read this customer's data.");
    }
}
