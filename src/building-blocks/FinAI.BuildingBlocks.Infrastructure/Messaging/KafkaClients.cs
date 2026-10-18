using Azure.Core;
using Azure.Identity;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace FinAI.BuildingBlocks.Infrastructure.Messaging;

public static class KafkaClients
{
    public static IProducer<string, byte[]> Producer(IConfiguration cfg)
    {
        var config = Apply(new ProducerConfig { Acks = Acks.All, EnableIdempotence = true }, cfg);
        var builder = new ProducerBuilder<string, byte[]>(config);
        if (UsesEntra(cfg)) builder.SetOAuthBearerTokenRefreshHandler((client, _) => SetToken(client, cfg));
        return builder.Build();
    }

    public static IConsumer<string, byte[]> Consumer(IConfiguration cfg, string groupId)
    {
        var config = Apply(new ConsumerConfig
        {
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        }, cfg);
        var builder = new ConsumerBuilder<string, byte[]>(config);
        if (UsesEntra(cfg)) builder.SetOAuthBearerTokenRefreshHandler((client, _) => SetToken(client, cfg));
        return builder.Build();
    }

    private static T Apply<T>(T config, IConfiguration cfg) where T : ClientConfig
    {
        config.BootstrapServers = cfg["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is not configured.");
        if (UsesEntra(cfg))
        {
            config.SecurityProtocol = SecurityProtocol.SaslSsl;
            config.SaslMechanism = SaslMechanism.OAuthBearer;
        }
        return config;
    }

    private static bool UsesEntra(IConfiguration cfg) => cfg["Kafka:Auth"] == "EntraId";

    private static void SetToken(IClient client, IConfiguration cfg)
    {
        var host = cfg["Kafka:BootstrapServers"]!.Split(':')[0];
        try
        {
            var token = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    { ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") })
                .GetToken(new TokenRequestContext([$"https://{host}/.default"]));
            client.OAuthBearerSetToken(token.Token, token.ExpiresOn.ToUnixTimeMilliseconds(), "finai");
        }
        catch (Exception ex)
        {
            client.OAuthBearerSetTokenFailure(ex.Message);
        }
    }
}
