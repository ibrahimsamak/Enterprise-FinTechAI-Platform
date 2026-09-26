namespace FinAI.BuildingBlocks.Application;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid CustomerId { get; }
    string Role { get; }
    bool IsAuthenticated { get; }
}
