using Serilog;
using FinAI.AI.Abstractions.Tools;
using FinAI.AI.Agents;
using FinAI.AI.Rag.Ingestion;
using FinAI.BuildingBlocks.Application;
using FinAI.BuildingBlocks.Infrastructure.Jobs;
using FinAI.BuildingBlocks.Infrastructure.Messaging;
using FinAI.Modules.Account.Application;
using FinAI.Modules.Account.Infrastructure;
using FinAI.Modules.Fraud;
using FinAI.Modules.Fraud.Infrastructure;
using FinAI.Modules.Payment.Infrastructure;
using FinAI.Workers.Jobs;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var ingest = args is ["ingest-knowledge", ..];

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((_, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341"));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("FinAI.Workers"))
    .WithTracing(t => t
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddNpgsql()
        .AddSource("FinAI.*")
        .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["OTEL:ENDPOINT"]!)))
    .WithMetrics(m => m
        .AddMeter("FinAI.*")
        .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["OTEL:ENDPOINT"]!)));

builder.Services.AddScoped<ICurrentUser, SystemCurrentUser>();
builder.Services.AddDbContext<AccountDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddScoped<IAccountQueries, AccountQueries>();

builder.Services.AddScoped<FraudRulesEngine>();
builder.Services.AddDbContext<FraudDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddFinAiAiPlatform(builder.Configuration);

builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
builder.Services.AddScoped<PaymentCreatedFraudHandler>();
builder.Services.AddScoped<PaymentCreatedTransactionProjection>();

if (!ingest)
{
    builder.Services.AddHostedService<OutboxDispatcher<FraudDbContext>>();

    builder.Services.AddHostedService<KafkaConsumerBackgroundService<PaymentCreatedIntegrationEvent, PaymentCreatedFraudHandler>>();
    builder.Services.AddHostedService<KafkaConsumerBackgroundService<PaymentCreatedIntegrationEvent, PaymentCreatedTransactionProjection>>();

    builder.Services.AddFinAiHangfire(builder.Configuration, runServer: true);
    builder.Services.AddDbContext<PaymentDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
}

var host = builder.Build();

if (!ingest)
{
    var jobs = host.Services.GetRequiredService<IRecurringJobManager>();
    jobs.AddOrUpdate<KnowledgeReindexJob>("knowledge-reindex", j => j.RunAsync(CancellationToken.None), Cron.Daily());
    jobs.AddOrUpdate<FraudCaseSlaJob>("fraud-case-sla", j => j.RunAsync(CancellationToken.None), "*/5 * * * *");
    jobs.AddOrUpdate<OutboxCleanupJob<PaymentDbContext>>("outbox-cleanup-payment", j => j.RunAsync(CancellationToken.None), Cron.Daily());
    jobs.AddOrUpdate<OutboxCleanupJob<FraudDbContext>>("outbox-cleanup-fraud", j => j.RunAsync(CancellationToken.None), Cron.Daily());
}

if (ingest)
{
    await host.StartAsync();
    using (var scope = host.Services.CreateScope())
    {
        var folder = KnowledgeBasePath.Find(args.Skip(1).FirstOrDefault(a => !a.StartsWith("--")) ?? "knowledge-base");
        var result = await scope.ServiceProvider.GetRequiredService<RagIngestionService>()
            .IngestDirectoryAsync(folder, args.Contains("--force"), CancellationToken.None);
        host.Services.GetRequiredService<ILogger<Program>>().LogInformation(
            "RAG ingest done: {Indexed} indexed, {Skipped} unchanged, {Removed} removed, {Chunks} chunks",
            result.Indexed, result.Skipped, result.Removed, result.Chunks);
    }
    await host.StopAsync();
    return;
}

host.Run();
