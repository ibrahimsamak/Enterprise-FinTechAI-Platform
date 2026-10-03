using FinAI.BuildingBlocks.Infrastructure;
using FinAI.Modules.Fraud.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinAI.Modules.Fraud.Infrastructure;

public sealed class FraudDbContext(DbContextOptions<FraudDbContext> o) : DbContext(o), IHasOutbox
{
    public DbSet<FraudCase> Cases => Set<FraudCase>();
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("fraud");
        b.Entity<FraudCase>(e =>
        {
            e.ComplexProperty(c => c.Amount, m =>
            {
                m.Property(x => x.Amount).HasColumnName("amount");
                m.Property(x => x.Currency).HasColumnName("currency");
            });
            e.HasIndex(c => c.PaymentId).IsUnique();
        });
        b.Entity<AuditRecord>(e =>
        {
            e.ToTable("audit_records");
            e.HasIndex(a => a.PaymentId);
        });
        b.Entity<OutboxMessage>(e =>
        {
            e.ToTable("outbox_messages");
            e.HasIndex(x => x.ProcessedOnUtc);
        });
    }
}
