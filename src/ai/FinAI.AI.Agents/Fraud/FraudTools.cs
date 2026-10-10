using FinAI.AI.Abstractions.Tools;
using FinAI.BuildingBlocks.Application;
using Microsoft.Extensions.AI;

namespace FinAI.AI.Agents.Fraud;

public sealed class FraudTools(IAccountQueries accounts, ICurrentUser user)
{
    public IList<AITool> For(Guid customerId, Guid paymentId) =>
    [
        AIFunctionFactory.Create(
            (CancellationToken ct) => accounts.GetRecentTransactions(customerId, user, paymentId, ct),
            "GetTransactionHistory",
            "Get this customer's previous outbound payments from the last 90 days (newest first, max 50). Excludes the payment under review."),
        AIFunctionFactory.Create(
            (CancellationToken ct) => accounts.GetLocations(customerId, user, paymentId, ct),
            "GetRecentLocations",
            "Get the destination countries this customer has previously paid to in the last 90 days, most frequent first. Excludes the payment under review.")
    ];
}
