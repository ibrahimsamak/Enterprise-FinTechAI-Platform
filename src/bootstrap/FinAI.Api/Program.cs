using FinAI.Api.ExceptionHandling;
using FinAI.BuildingBlocks.Application;
using FinAI.BuildingBlocks.Application.Behaviors;
using FinAI.BuildingBlocks.Application.Messaging;
using FinAI.BuildingBlocks.Infrastructure.Messaging;
using System.Text.Json.Serialization;
using FinAI.Modules.Payment.Application;
using FinAI.Modules.Payment.Infrastructure;
using MediatR;
using Serilog;
using FinAI.Modules.Identity;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Npgsql;
using StackExchange.Redis;
using FinAI.Modules.Notification;
using FluentValidation;
using FinAI.BuildingBlocks.Infrastructure.Jobs;
using Hangfire;
using FinAI.Api.Dashboard;
using FinAI.AI.Agents;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341"));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("FinAI.Api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddNpgsql()
        .AddSource("FinAI.*")
        .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["OTEL:ENDPOINT"]!)))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["OTEL:ENDPOINT"]!)));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddFinAIAuth(builder.Configuration);

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

builder.Services.AddMediatR(c => c.RegisterServicesFromAssemblies(ModuleRegistry.Assemblies));
builder.Services.AddValidatorsFromAssemblies(ModuleRegistry.Assemblies);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));  

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
builder.Services.AddHostedService<OutboxDispatcher<PaymentDbContext>>();

var modules = ModuleRegistry.LoadModules();
foreach (var m in modules) m.Register(builder.Services, builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddScoped<FraudFlaggedRelayHandler>();
builder.Services.AddHostedService<KafkaConsumerBackgroundService<FraudFlaggedIntegrationEvent, FraudFlaggedRelayHandler>>();

builder.Services.AddScoped<PaymentClearedHandler>();
builder.Services.AddHostedService<KafkaConsumerBackgroundService<PaymentClearedIntegrationEvent, PaymentClearedHandler>>();
builder.Services.AddScoped<FraudFlaggedPaymentHoldHandler>();
builder.Services.AddHostedService<KafkaConsumerBackgroundService<FraudFlaggedIntegrationEvent, FraudFlaggedPaymentHoldHandler>>();
builder.Services.AddScoped<FraudCaseResolvedPaymentHandler>();
builder.Services.AddHostedService<KafkaConsumerBackgroundService<FraudCaseResolvedIntegrationEvent, FraudCaseResolvedPaymentHandler>>();

builder.Services.AddScoped<DashboardReadModel>();
builder.Services.AddGraphQLServer().AddAuthorization().AddQueryType<DashboardQuery>();

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

builder.Services.AddFinAiHangfire(builder.Configuration, runServer: false);

builder.Services.AddFinAiAiCore(builder.Configuration);
builder.Services.AddFinAiCustomerSupportAgent();

var app = builder.Build();
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
foreach (var m in modules) m.MapEndpoints(app);
app.MapHub<FraudHub>("/hubs/fraud");
app.MapHub<SupportHub>("/hubs/support");

app.MapHangfireDashboard("/hangfire");
app.MapGraphQL().RequireAuthorization("FraudOfficer");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapDevTokenEndpoint();
}

app.Run();
