using FinAI.BuildingBlocks.Application;
using FinAI.BuildingBlocks.Application.Messaging;
using FinAI.BuildingBlocks.Infrastructure;
using FinAI.Modules.Fraud.Domain;
using FinAI.Modules.Fraud.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinAI.Modules.Fraud.Application;

public sealed record ResolveFraudCaseCommand(Guid PaymentId, FraudResolution Resolution, string Notes) : ICommand;

public sealed class ResolveFraudCaseValidator : AbstractValidator<ResolveFraudCaseCommand>
{
    public ResolveFraudCaseValidator()
    {
        RuleFor(x => x.Resolution).IsInEnum();
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(2000);
    }
}

public sealed class ResolveFraudCaseHandler(FraudDbContext db, ICurrentUser user) : IRequestHandler<ResolveFraudCaseCommand>
{
    public async Task Handle(ResolveFraudCaseCommand c, CancellationToken ct)
    {
        var fraudCase = await db.Cases.SingleOrDefaultAsync(x => x.PaymentId == c.PaymentId, ct)
            ?? throw new NotFoundException($"No fraud case for payment {c.PaymentId}.");

        fraudCase.Resolve(c.Resolution, user.UserId);

        db.AuditRecords.Add(new AuditRecord
        {
            Action = $"fraud.{c.Resolution}",
            PaymentId = c.PaymentId,
            ActorId = user.UserId,
            Notes = c.Notes
        });
        db.Outbox.Enqueue(new FraudCaseResolvedIntegrationEvent(fraudCase.Id, c.PaymentId, c.Resolution, user.UserId));

        await db.SaveChangesAsync(ct);
    }
}
