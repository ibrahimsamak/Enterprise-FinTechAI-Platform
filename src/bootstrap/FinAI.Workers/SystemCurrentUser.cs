using FinAI.BuildingBlocks.Application;

public sealed class SystemCurrentUser : ICurrentUser
{
    public Guid? UserId => null;
    public Guid CustomerId => Guid.Empty;
    public string Role => "FraudSystem";
    public bool IsAuthenticated => true;
}