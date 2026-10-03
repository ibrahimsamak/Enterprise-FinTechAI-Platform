using FinAI.AI.Abstractions.Fraud;
using FinAI.BuildingBlocks.Application;

namespace FinAI.Modules.Fraud.Domain;

public sealed class FraudCase : AggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PaymentId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Money Amount { get; private set; }
    public string Country { get; private set; } = default!;
    public double Score { get; private set; }
    public List<string> Reasons { get; private set; } = [];
    public string Investigation { get; private set; } = default!;  
    public FraudCaseStatus Status { get; private set; }
    public DateTime CreatedOnUtc { get; private set; } = DateTime.UtcNow;
    public Guid? ResolvedBy { get; private set; }
    public DateTime? ResolvedOnUtc { get; private set; }

    public static FraudCase Open(PaymentCreatedIntegrationEvent e, FraudDecision decision, string investigation)
        => new()
        {
            PaymentId = e.PaymentId,
            CustomerId = e.CustomerId,
            Amount = Money.Of(e.Amount, e.Currency),
            Country = e.Country,
            Score = decision.Score,
            Reasons = [.. decision.Reasons],
            Investigation = investigation,
            Status = FraudCaseStatus.Open
        };

    public void Resolve(FraudResolution resolution, Guid? officerId)
    {
        if (Status != FraudCaseStatus.Open)
            throw new ConflictException($"Fraud case for payment {PaymentId} is already {Status}.");

        Status = resolution == FraudResolution.Approve ? FraudCaseStatus.Approved : FraudCaseStatus.Escalated;
        ResolvedBy = officerId;
        ResolvedOnUtc = DateTime.UtcNow;
    }

    private FraudCase() { }
}
public enum FraudCaseStatus { Open, Approved, Escalated }
