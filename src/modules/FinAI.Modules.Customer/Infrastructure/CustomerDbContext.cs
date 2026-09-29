using Microsoft.EntityFrameworkCore;

namespace FinAI.Modules.Customer.Infrastructure;

public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> o) : DbContext(o)
{
    public DbSet<Domain.Customer> Customers => Set<Domain.Customer>();
    protected override void OnModelCreating(ModelBuilder b) => b.HasDefaultSchema("customer");
}
