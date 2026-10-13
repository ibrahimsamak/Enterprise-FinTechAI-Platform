using HotChocolate;
using HotChocolate.Authorization;

namespace FinAI.Api.Dashboard;

[Authorize(Policy = "FraudOfficer")]
public sealed class DashboardQuery
{
    public Task<DashboardDto> GetDashboard([Service] DashboardReadModel rm, CancellationToken ct) => rm.LoadAsync(ct);
}

public sealed record DashboardDto(
    int CustomerCount,
    IReadOnlyList<VolumeDto> PaymentVolumeToday,
    int PaymentsOnHold,
    int OpenFraudCases,
    int FraudCasesResolvedToday);

public sealed record VolumeDto(string Currency, decimal Amount, int Count);
