

[Topic("payment.created")]
public sealed record PaymentCreatedIntegrationEvent(
        Guid PaymentId, Guid AccountId, Guid CustomerId,
        decimal Amount, string Currency, string Country
): IntegrationEvent
{
    public override string PartitionKey => PaymentId.ToString();
}
