using FinAI.BuildingBlocks.Application;
using Microsoft.AspNetCore.Http;

namespace FinAI.Modules.Identity;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId => accessor.HttpContext?.User?.GetUserId();
    public Guid CustomerId => accessor.HttpContext?.User?.GetCustomerId() ?? Guid.Empty;
    public string Role => accessor.HttpContext?.User?.GetRole() ?? string.Empty;
    public bool IsAuthenticated => accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
