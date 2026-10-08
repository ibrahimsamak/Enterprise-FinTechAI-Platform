using FinAI.BuildingBlocks.Application;

namespace FinAI.AI.Abstractions.Tools;

public interface IAccountQueries
{
    Task<IReadOnlyList<TxnDto>> GetRecentTransactions(Guid customerId, ICurrentUser caller, Guid? excludePaymentId = null, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetLocations(Guid customerId, ICurrentUser caller, Guid? excludePaymentId = null, CancellationToken ct = default);
}

public sealed record TxnDto(Guid Id, decimal Amount, string Currency, string Country, DateTime OccurredOnUtc);
