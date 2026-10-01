using FinAI.Modules.Account.Infrastructure.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace FinAI.Modules.Account.Infrastructure;

public sealed class AccountDbContext(DbContextOptions<AccountDbContext> o) : DbContext(o)
{
    public DbSet<Domain.Account> Accounts => Set<Domain.Account>();
    public DbSet<CustomerTransaction> Transactions => Set<CustomerTransaction>();   

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("account");
        b.Entity<Domain.Account>(e =>
        {
            e.ComplexProperty(a => a.Balance, m =>
            {
                m.Property(p => p.Amount).HasColumnName("balance_amount");
                m.Property(p => p.Currency).HasColumnName("balance_currency");
            });
            e.Property(a => a.RowVersion).IsRowVersion();
        });
        b.Entity<CustomerTransaction>(e =>
        {
            e.ToTable("customer_transactions");
            e.HasKey(t => t.PaymentId);
            e.Property(t => t.PaymentId).ValueGeneratedNever();
            e.Property(t => t.Amount).HasPrecision(18, 2);
            e.HasIndex(t => new { t.CustomerId, t.OccurredOnUtc });
        });
    }
}
