using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace FinAI.ServiceDefaults;

public static class ServiceDefaults
{
    public static IHostApplicationBuilder AddFinAiDefaults(this IHostApplicationBuilder builder, string serviceName, bool isWeb)
    {
        var cfg = builder.Configuration;

        if (cfg["AZURE:KEYVAULTURI"] is { Length: > 0 } vault)
            cfg.AddAzureKeyVault(new Uri(vault), AzureCredentials.Default);

        builder.Services.AddSerilog((_, lc) =>
        {
            lc.ReadFrom.Configuration(cfg).WriteTo.Console();
            if (cfg["SEQ:URL"] is { Length: > 0 } seq) lc.WriteTo.Seq(seq);
        }, writeToProviders: true);

        var otlp = cfg["OTEL:ENDPOINT"];
        var appInsights = cfg["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(t =>
            {
                if (isWeb) t.AddAspNetCoreInstrumentation();
                t.AddHttpClientInstrumentation()
                 .AddEntityFrameworkCoreInstrumentation()
                 .AddNpgsql()
                 .AddSource("FinAI.*");
                if (otlp is { Length: > 0 }) t.AddOtlpExporter(o => o.Endpoint = new Uri(otlp));
                if (appInsights is { Length: > 0 }) t.AddAzureMonitorTraceExporter(o => o.ConnectionString = appInsights);
            })
            .WithMetrics(m =>
            {
                if (isWeb) m.AddAspNetCoreInstrumentation();
                m.AddRuntimeInstrumentation()
                 .AddMeter("FinAI.*");
                if (otlp is { Length: > 0 }) m.AddOtlpExporter(o => o.Endpoint = new Uri(otlp));
                if (appInsights is { Length: > 0 }) m.AddAzureMonitorMetricExporter(o => o.ConnectionString = appInsights);
            });

        if (appInsights is { Length: > 0 })
            builder.Logging.AddOpenTelemetry(l => l.AddAzureMonitorLogExporter(o => o.ConnectionString = appInsights));

        return builder;
    }
}

public static class AzureCredentials
{
    public static readonly Azure.Core.TokenCredential Default = new DefaultAzureCredential(
        new DefaultAzureCredentialOptions { ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") });
}
