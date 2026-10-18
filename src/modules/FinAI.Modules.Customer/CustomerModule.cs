using FinAI.BuildingBlocks.Application;
using FinAI.Modules.Customer.Application;
using FinAI.Modules.Customer.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace FinAI.Modules.Customer;

public sealed class CustomerModule : IModule
{
    public void Register(IServiceCollection s, IConfiguration cfg)
    {
        s.AddDbContext<CustomerDbContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
        s.AddScoped<ICustomerLookup, CustomerLookup>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/customers").RequireAuthorization();
        g.MapPost("/", async (CreateCustomerCommand c, ISender sender) =>
            Results.Ok(await sender.Send(c)));
    }
}
