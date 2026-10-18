using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Pgvector.Npgsql;

namespace FinAI.BuildingBlocks.Infrastructure.Data;

public static class PostgresExtensions
{
    private static readonly string[] Scope = ["https://ossrdbms-aad.database.windows.net/.default"];

    public static IServiceCollection AddFinAiPostgres(this IServiceCollection s, IConfiguration cfg, TokenCredential? credential = null)
    {
        var builder = new NpgsqlDataSourceBuilder(cfg.GetConnectionString("Postgres"));
        builder.UseVector();

        if (credential is not null && string.IsNullOrEmpty(builder.ConnectionStringBuilder.Password))
            builder.UsePeriodicPasswordProvider(
                async (_, ct) => (await credential.GetTokenAsync(new TokenRequestContext(Scope), ct)).Token,
                successRefreshInterval: TimeSpan.FromMinutes(50),
                failureRefreshInterval: TimeSpan.FromSeconds(10));

        s.AddSingleton(builder.Build());
        return s;
    }
}
