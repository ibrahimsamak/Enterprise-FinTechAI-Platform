
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Of(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency cannot be null or whitespace.", nameof(currency));
        }
        return new Money(amount, currency);
    }
    public Money Add(Money other) => SameCurrency(other) with { Amount = Amount + other.Amount };
    public Money Subtract(Money other) => SameCurrency(other) with { Amount = Amount - other.Amount };
    private Money SameCurrency(Money o) => o.Currency == Currency ? this : throw new InvalidOperationException("Currency mismatch.");
}