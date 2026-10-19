using FinAI.AI.Rag;
using FinAI.BuildingBlocks.Infrastructure.Data;
using FinAI.Modules.Account.Infrastructure;
using FinAI.Modules.Customer.Infrastructure;
using FinAI.Modules.Fraud.Infrastructure;
using FinAI.Modules.Payment.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Pgvector.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFinAiPostgres(builder.Configuration);

void Add<T>() where T : DbContext =>
    builder.Services.AddDbContext<T>((sp, o) =>
    o.UseNpgsql(
        sp.GetRequiredService<NpgsqlDataSource>(),
        npg => npg.UseVector()
    ));

Add<CustomerDbContext>();
Add<AccountDbContext>();
Add<PaymentDbContext>();
Add<FraudDbContext>();
Add<RagDbContext>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();
foreach (var type in new[] { typeof(CustomerDbContext), typeof(AccountDbContext), typeof(PaymentDbContext), typeof(FraudDbContext), typeof(RagDbContext) })
{
    var db = (DbContext)scope.ServiceProvider.GetRequiredService(type);
    Console.WriteLine($"Migrating {type.Name}...");
    await db.Database.MigrateAsync();
}