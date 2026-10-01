namespace FinAI.Modules.Account.Domain;

public sealed class Account : AggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CustomerId { get; private set; }
    public Money Balance { get; private set; }
    public AccountStatus Status { get; private set; }
    public uint RowVersion { get; private set; }

    public static Account Open(Guid customerId, string currency)
        => new() { CustomerId = customerId, Balance = Money.Of(0, currency), Status = AccountStatus.Active };

    public void Debit(Money amount)
    {
        if (Status != AccountStatus.Active) throw new InvalidOperationException("Account not active.");
        if (Balance.Amount < amount.Amount) throw new InvalidOperationException("Insufficient funds.");
        Balance = Balance.Subtract(amount);
    }
    public void Credit(Money amount) => Balance = Balance.Add(amount);
    private Account() { }
}
public enum AccountStatus { Active, Frozen, Closed }

