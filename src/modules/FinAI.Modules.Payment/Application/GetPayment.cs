using FinAI.BuildingBlocks.Application;
using FinAI.BuildingBlocks.Application.Messaging;
using FinAI.Modules.Payment.Domain;
using FinAI.Modules.Payment.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinAI.Modules.Payment.Application;

public sealed record PaymentDto(
    Guid Id, Guid AccountId, decimal Amount, string Currency, string Country,
    PaymentStatus Status, DateTime CreatedOnUtc);

public sealed record GetPaymentQuery(Guid PaymentId) : IQuery<PaymentDto>;

public sealed class GetPaymentHandler(PaymentDbContext db, ICurrentUser user) : IQueryHandler<GetPaymentQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(GetPaymentQuery q, CancellationToken ct)
    {
        var isOfficer = user.Role == "FraudOfficer";
        var customerId = user.CustomerId;

        return await db.Payments.AsNoTracking()
                   .Where(p => p.Id == q.PaymentId && (isOfficer || p.CustomerId == customerId))
                   .Select(p => new PaymentDto(p.Id, p.AccountId, p.Amount.Amount, p.Amount.Currency,
                                               p.Country, p.Status, p.CreatedOnUtc))
                   .SingleOrDefaultAsync(ct)
               ?? throw new NotFoundException($"Payment {q.PaymentId} not found.");
    }
}
