using FinAI.BuildingBlocks.Application;
using FinAI.BuildingBlocks.Application.Messaging;
using FinAI.BuildingBlocks.Infrastructure;
using FinAI.Modules.Payment.Infrastructure;
using MediatR;

namespace FinAI.Modules.Payment.Application;

public sealed record CreatePaymentCommand(Guid AccountId, decimal Amount, string Currency, string Country) : ICommand<Guid>;

public sealed class CreatePaymentHandler(PaymentDbContext db, ICurrentUser user) : IRequestHandler<CreatePaymentCommand, Guid>
{
    public async Task<Guid> Handle(CreatePaymentCommand c, CancellationToken ct)
    {
        var payment = Domain.Payment.Create(c.AccountId, user.CustomerId, Money.Of(c.Amount, c.Currency), c.Country);
        db.Payments.Add(payment);

        db.Outbox.Enqueue(new PaymentCreatedIntegrationEvent(
            payment.Id, c.AccountId, user.CustomerId, c.Amount, c.Currency, c.Country));

        await db.SaveChangesAsync(ct);
        return payment.Id;
    }
}
