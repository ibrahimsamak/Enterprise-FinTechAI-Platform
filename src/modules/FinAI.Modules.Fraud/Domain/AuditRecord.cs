namespace FinAI.Modules.Fraud.Domain;

public sealed class AuditRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Action { get; init; } = default!;
    public Guid PaymentId { get; init; }
    public Guid? ActorId { get; init; }
    public string Notes { get; init; } = default!;
    public DateTime AtUtc { get; init; } = DateTime.UtcNow;
}
