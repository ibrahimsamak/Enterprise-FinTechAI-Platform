namespace FinAI.AI.Abstractions.Fraud;

public interface IFraudAgent
{
    Task<string> InvestigateAsync(PaymentCreatedIntegrationEvent e, FraudDecision decision, CancellationToken ct = default);
}

public sealed record FraudDecision(bool IsHighRisk, double Score, IReadOnlyList<string> Reasons);
