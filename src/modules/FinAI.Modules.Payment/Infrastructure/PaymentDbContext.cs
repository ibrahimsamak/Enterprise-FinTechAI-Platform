using FinAI.BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinAI.Modules.Payment.Infrastructure;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> o) : DbContext(o), IHasOutbox
{
    public DbSet<Domain.Payment> Payments => Set<Domain.Payment>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("payment");
        b.Entity<Domain.Payment>(e =>
        {
            e.ComplexProperty(p => p.Amount, m =>
            {
                m.Property(x => x.Amount).HasColumnName("amount");
                m.Property(x => x.Currency).HasColumnName("currency");
            });
            e.HasIndex(p => p.CustomerId);
        });
        b.Entity<OutboxMessage>(e =>
        {
            e.ToTable("outbox_messages");
            e.HasIndex(x => x.ProcessedOnUtc);
        });
    }
}
