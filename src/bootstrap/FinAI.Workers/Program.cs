using Serilog;
using FinAI.AI.Abstractions.Tools;
using FinAI.AI.Agents;
using FinAI.AI.Rag.Ingestion;
using FinAI.BuildingBlocks.Application;
using FinAI.BuildingBlocks.Infrastructure.Data;
using FinAI.BuildingBlocks.Infrastructure.Jobs;
using FinAI.BuildingBlocks.Infrastructure.Messaging;
using FinAI.Modules.Account.Application;
using FinAI.Modules.Account.Infrastructure;
using FinAI.Modules.Fraud;
using FinAI.Modules.Fraud.Infrastructure;
using FinAI.Modules.Payment.Infrastructure;
using FinAI.Workers.Jobs;
using FinAI.ServiceDefaults;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var ingest = args is ["ingest-knowledge", ..];

var builder = Host.CreateApplicationBuilder(args);

builder.AddFinAiDefaults("FinAI.Workers", isWeb: false);

builder.Services.AddFinAiPostgres(builder.Configuration, AzureCredentials.Default);

builder.Services.AddScoped<ICurrentUser, SystemCurrentUser>();
builder.Services.AddDbContext<AccountDbContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
builder.Services.AddScoped<IAccountQueries, AccountQueries>();

builder.Services.AddScoped<FraudRulesEngine>();
builder.Services.AddDbContext<FraudDbContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
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
    builder.Services.AddDbContext<PaymentDbContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
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
