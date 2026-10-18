using FinAI.BuildingBlocks.Application;
using FinAI.BuildingBlocks.Application.Messaging;
using FinAI.Modules.Customer.Infrastructure;
using FluentValidation;
using MediatR;

namespace FinAI.Modules.Customer.Application;

public sealed record CreateCustomerCommand(string FullName, string Email) : ICommand<Guid>;

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.FullName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public sealed class CreateCustomerHandler(CustomerDbContext db, ICurrentUser currentUser) : IRequestHandler<CreateCustomerCommand, Guid>
{
    public async Task<Guid> Handle(CreateCustomerCommand cmd, CancellationToken cancellationToken)
    {
        var customer = Domain.Customer.Create(cmd.FullName, cmd.Email, currentUser.UserId);
        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);
        return customer.Id;
    }
}
