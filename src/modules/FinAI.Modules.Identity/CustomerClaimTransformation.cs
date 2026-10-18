using System.Security.Claims;
using FinAI.BuildingBlocks.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace FinAI.Modules.Identity;

public sealed class CustomerClaimTransformation(ICustomerLookup lookup, IMemoryCache cache) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.HasClaim(c => c.Type == ClaimsPrincipalExtensions.CustomerIdClaim)) return principal;
        if (!principal.IsInRole("Customer") || principal.GetUserId() is not { } userId) return principal;

        var customerId = await cache.GetOrCreateAsync($"customer-of:{userId}", e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return lookup.FindCustomerIdAsync(userId, CancellationToken.None);
        });
        if (customerId is null) return principal;

        var identity = new ClaimsIdentity([new Claim(ClaimsPrincipalExtensions.CustomerIdClaim, customerId.ToString()!)]);
        principal.AddIdentity(identity);
        return principal;
    }
}
