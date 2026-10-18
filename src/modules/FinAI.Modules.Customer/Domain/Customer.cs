namespace FinAI.Modules.Customer.Domain;

public sealed class Customer : AggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;

    public Guid? UserId { get; private set; }

    public static Customer Create(string name, string email, Guid? userId = null)
        => new() { FullName = name, Email = email, UserId = userId };
    private Customer() { }
}

