namespace FinAI.Modules.Customer.Domain;

public sealed class Customer : AggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public static Customer Create(string name, string email)
        => new() { FullName = name, Email = email };
    private Customer() { }
}

