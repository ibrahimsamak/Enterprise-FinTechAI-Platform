using FinAI.Modules.Customer.Infrastructure;
using FinAI.Modules.Fraud.Domain;
using FinAI.Modules.Fraud.Infrastructure;
using FinAI.Modules.Payment.Domain;
using FinAI.Modules.Payment.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinAI.Api.Dashboard;

public sealed class DashboardReadModel(CustomerDbContext customers, PaymentDbContext payments, FraudDbContext fraud)
{
    public async Task<DashboardDto> LoadAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        var customerCount = await customers.Customers.CountAsync(ct);

        var volume = await payments.Payments.AsNoTracking()
            .Where(p => p.CreatedOnUtc >= today)
            .Select(p => new { p.Amount.Currency, p.Amount.Amount })
            .GroupBy(p => p.Currency)
            .Select(g => new VolumeDto(g.Key, g.Sum(p => p.Amount), g.Count()))
            .ToListAsync(ct);

        var onHold = await payments.Payments.CountAsync(p => p.Status == PaymentStatus.OnHold, ct);

        var openCases = await fraud.Cases.CountAsync(c => c.Status == FraudCaseStatus.Open, ct);
        var resolvedToday = await fraud.Cases.CountAsync(c => c.ResolvedOnUtc >= today, ct);

        return new DashboardDto(customerCount, volume, onHold, openCases, resolvedToday);
    }
}
