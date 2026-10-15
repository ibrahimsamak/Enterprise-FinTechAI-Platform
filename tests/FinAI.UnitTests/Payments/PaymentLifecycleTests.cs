using FinAI.AI.Abstractions.Fraud;
using FinAI.BuildingBlocks.Application;
using FinAI.Modules.Fraud.Domain;
using FinAI.Modules.Payment.Domain;

namespace FinAI.UnitTests.Payments;

public class PaymentLifecycleTests
{
    private static Payment NewPayment() => Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Money.Of(9500, "CAD"), "DE");

    [Fact]
    public void Payment_needs_its_customer() =>
        Assert.Throws<ArgumentException>(() => Payment.Create(Guid.NewGuid(), Guid.Empty, Money.Of(50, "CAD"), "CA"));

    [Fact]
    public void Cleared_payment_completes()
    {
        var p = NewPayment();
        p.Complete();
        Assert.Equal(PaymentStatus.Completed, p.Status);
    }

    [Fact]
    public void Flagged_payment_is_held_then_completed_on_approval()
    {
        var p = NewPayment();
        p.Hold();
        Assert.Equal(PaymentStatus.OnHold, p.Status);
        p.Complete();
        Assert.Equal(PaymentStatus.Completed, p.Status);
    }

    [Fact]
    public void Late_hold_does_not_reopen_a_completed_payment()
    {
        var p = NewPayment();
        p.Complete();
        p.Hold();
        Assert.Equal(PaymentStatus.Completed, p.Status);
    }

    [Fact]
    public void Fraud_case_is_resolved_once()
    {
        var e = new PaymentCreatedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 9500, "CAD", "DE");
        var c = FraudCase.Open(e, new FraudDecision(true, 1.0, ["Amount significantly above typical."]), "summary");
        var officer = Guid.NewGuid();

        c.Resolve(FraudResolution.Approve, officer);

        Assert.Equal(FraudCaseStatus.Approved, c.Status);
        Assert.Equal(officer, c.ResolvedBy);
        Assert.Throws<ConflictException>(() => c.Resolve(FraudResolution.Escalate, officer));
    }
}
