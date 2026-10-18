namespace FinAI.BuildingBlocks.Application;

public interface ICustomerLookup
{
    Task<Guid?> FindCustomerIdAsync(Guid userId, CancellationToken ct);
}
