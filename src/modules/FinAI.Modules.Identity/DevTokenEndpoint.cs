using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace FinAI.Modules.Identity;

public sealed record DevTokenRequest(Guid UserId, string Role, Guid? CustomerId);

public static class DevTokenEndpoint
{
    private static readonly string[] Roles = ["Customer", "FraudOfficer"];

    public static IEndpointConventionBuilder MapDevTokenEndpoint(this IEndpointRouteBuilder app) =>
        app.MapPost("/dev/token", (DevTokenRequest req, IConfiguration cfg) =>
        {
            if (!Roles.Contains(req.Role))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                    { ["role"] = [$"Role must be one of: {string.Join(", ", Roles)}."] });
            if (req.Role == "Customer" && (req.CustomerId ?? Guid.Empty) == Guid.Empty)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                    { ["customerId"] = ["A Customer token needs a customerId."] });

            return Results.Ok(TokenIssuer.IssueToken(req.UserId, req.Role, req.CustomerId, cfg));
        }).AllowAnonymous();
}
