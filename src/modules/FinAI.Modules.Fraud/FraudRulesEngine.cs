using FinAI.AI.Abstractions.Fraud;

namespace FinAI.Modules.Fraud;

public sealed class FraudRulesEngine
{
    public FraudDecision Evaluate(PaymentCreatedIntegrationEvent e)
    {
        var reasons = new List<string>();
        if (e.Amount >= 5000) reasons.Add("Amount significantly above typical.");
        if (e.Country is not "CA") reasons.Add($"Unusual destination: {e.Country}.");
        if (e.OccurredOn.Hour is >= 0 and <= 5) reasons.Add("Unusual transaction time.");
        var score = reasons.Count / 3.0;
        return new FraudDecision(score >= 0.66, score, reasons);
    }
}