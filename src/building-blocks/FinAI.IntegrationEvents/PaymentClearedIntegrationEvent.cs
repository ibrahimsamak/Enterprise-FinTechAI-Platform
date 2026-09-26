[Topic("payment.cleared")]
public sealed record PaymentClearedIntegrationEvent(Guid PaymentId, double Score) : IntegrationEvent
{
    public override string PartitionKey => PaymentId.ToString();
}
