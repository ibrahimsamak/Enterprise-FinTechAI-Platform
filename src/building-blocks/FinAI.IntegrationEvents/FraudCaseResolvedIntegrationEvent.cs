public enum FraudResolution { Approve, Escalate }

[Topic("fraud.case.resolved")]
public sealed record FraudCaseResolvedIntegrationEvent(
        Guid CaseId, Guid PaymentId, FraudResolution Resolution, Guid? ResolvedBy
): IntegrationEvent
{
    public override string PartitionKey => PaymentId.ToString();
}
