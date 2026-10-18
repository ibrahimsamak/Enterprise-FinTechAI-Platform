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
using Hangfire.Dashboard;
using Microsoft.Identity.Web;
using FinAI.Api.Dashboard;
using FinAI.AI.Agents;
using FinAI.ServiceDefaults;
using FinAI.BuildingBlocks.Infrastructure.Data;
using Microsoft.Azure.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.AddFinAiDefaults("FinAI.Api", isWeb: true);

builder.Services.AddFinAiPostgres(builder.Configuration, AzureCredentials.Default);

builder.Services.AddHealthChecks().AddDbContextCheck<PaymentDbContext>("postgres", tags: ["ready"]);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddFinAIAuth(builder.Configuration);

if (builder.Configuration.GetSection("AzureAd").Exists())
{
    builder.Services.AddAuthentication()
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"),
            openIdConnectScheme: "HangfireOidc", cookieScheme: "HangfireCookie");

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("HangfireAdmin", p => p
            .AddAuthenticationSchemes("HangfireOidc")
            .RequireRole("Admin"));
}

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

var signalR = builder.Services.AddSignalR();
if (builder.Configuration["AZURE:SIGNALR:ENDPOINT"] is { Length: > 0 } endpoint)
    signalR.AddAzureSignalR(o => o.Endpoints = [new ServiceEndpoint(new Uri(endpoint), AzureCredentials.Default)]);

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

if (app.Configuration.GetSection("AzureAd").Exists())
    app.MapHangfireDashboard("/hangfire", new DashboardOptions { Authorization = [] })
       .RequireAuthorization("HangfireAdmin");
else
    app.MapHangfireDashboard("/hangfire");
app.MapGraphQL().RequireAuthorization("FraudOfficer");

app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = c => c.Tags.Contains("ready") });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapDevTokenEndpoint();
}

app.Run();
