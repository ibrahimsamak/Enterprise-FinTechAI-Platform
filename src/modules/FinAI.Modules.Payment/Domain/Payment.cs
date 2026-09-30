namespace FinAI.Modules.Payment.Domain;

public sealed class Payment : AggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Money Amount { get; private set; }
    public string Country { get; private set; } = default!;
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedOnUtc { get; private set; } = DateTime.UtcNow;

    public static Payment Create(Guid accountId, Guid customerId, Money amount, string country)
    {    
        if (customerId == Guid.Empty) throw new ArgumentException("A payment needs its customer.", nameof(customerId));

        if (amount.Amount <= 0) throw new ArgumentException("Payment amount must be positive.", nameof(amount));
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country is required.", nameof(country));
        return new() { AccountId = accountId, CustomerId = customerId, Amount = amount, Country = country, Status = PaymentStatus.Pending };
    }

    public void Complete()
    {
        if (Status is PaymentStatus.Pending or PaymentStatus.OnHold) Status = PaymentStatus.Completed;
    }

    public void Hold()
    {
        if (Status is PaymentStatus.Pending) Status = PaymentStatus.OnHold;
    }

    private Payment() { }
}
public enum PaymentStatus { Pending, OnHold, Completed, Rejected }