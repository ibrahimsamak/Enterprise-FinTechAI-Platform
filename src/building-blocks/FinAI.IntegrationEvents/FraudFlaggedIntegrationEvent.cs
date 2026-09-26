[Topic("fraud.flagged")]
public sealed record FraudFlaggedIntegrationEvent(
        Guid PaymentId, Guid CustomerId,
        decimal Amount, string Currency, string Country,
        double Score, string[] Reasons, string Investigation
): IntegrationEvent
{
    public override string PartitionKey => PaymentId.ToString();
}
