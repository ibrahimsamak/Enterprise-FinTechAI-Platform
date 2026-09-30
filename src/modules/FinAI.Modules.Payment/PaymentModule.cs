using FinAI.Modules.Payment.Application;
using FinAI.Modules.Payment.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinAI.Modules.Payment;

public sealed class PaymentModule : IModule
{
    public void Register(IServiceCollection s, IConfiguration cfg) =>
        s.AddDbContext<PaymentDbContext>(o => o.UseNpgsql(cfg.GetConnectionString("Postgres")));

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/payments").RequireAuthorization();

        g.MapPost("/", async (CreatePaymentCommand c, ISender sender) =>
            Results.Ok(await sender.Send(c))).RequireAuthorization("Customer");

        g.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new GetPaymentQuery(id))));
    }
}
