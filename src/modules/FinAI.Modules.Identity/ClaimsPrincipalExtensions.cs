using System.Security.Claims;
using Microsoft.Identity.Web;

namespace FinAI.Modules.Identity;

public static class ClaimsPrincipalExtensions
{
    public const string CustomerIdClaim = "customerId";

    public static Guid? GetUserId(this ClaimsPrincipal user)
        => Guid.TryParse(user.GetObjectId() ?? user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public static Guid GetCustomerId(this ClaimsPrincipal user)
        => Guid.TryParse(user.FindFirstValue(CustomerIdClaim), out var id) ? id : Guid.Empty;

    public static string GetRole(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue("roles") ?? string.Empty;
}
