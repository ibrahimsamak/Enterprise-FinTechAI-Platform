using FinAI.Modules.Fraud.Application;
using FinAI.Modules.Fraud.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinAI.Modules.Fraud;

public sealed class FraudModule : IModule
{
    public void Register(IServiceCollection s, IConfiguration cfg) =>
        s.AddDbContext<FraudDbContext>(o => o.UseNpgsql(cfg.GetConnectionString("Postgres")));

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/fraud-cases").RequireAuthorization("FraudOfficer");
        g.MapPost("/{paymentId:guid}/resolve", async (Guid paymentId, ResolveFraudCaseRequest body, ISender sender) =>
        {
            await sender.Send(new ResolveFraudCaseCommand(paymentId, body.Resolution, body.Notes));
            return Results.NoContent();
        });
    }
}

public sealed record ResolveFraudCaseRequest(FraudResolution Resolution, string Notes);
