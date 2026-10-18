using FinAI.BuildingBlocks.Application;
using Microsoft.EntityFrameworkCore;

namespace FinAI.Modules.Customer.Infrastructure;

public sealed class CustomerLookup(CustomerDbContext db) : ICustomerLookup
{
    public async Task<Guid?> FindCustomerIdAsync(Guid userId, CancellationToken ct)
        => await db.Customers
            .Where(c => c.UserId == userId)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);
}
