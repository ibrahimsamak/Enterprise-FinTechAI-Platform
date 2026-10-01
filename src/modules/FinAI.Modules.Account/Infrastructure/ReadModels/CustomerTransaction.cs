namespace FinAI.Modules.Account.Infrastructure.ReadModels;

public sealed class CustomerTransaction
{
    public Guid PaymentId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string Country { get; set; } = default!;
    public DateTime OccurredOnUtc { get; set; }
}
