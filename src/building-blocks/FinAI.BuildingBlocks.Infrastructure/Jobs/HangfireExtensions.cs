using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace FinAI.BuildingBlocks.Infrastructure.Jobs;

sealed class DataSourceConnectionFactory(NpgsqlDataSource ds) : IConnectionFactory
{
    public NpgsqlConnection GetOrCreateConnection() => ds.CreateConnection();
}

public static class HangfireExtensions
{
    public static IServiceCollection AddFinAiHangfire(this IServiceCollection s, IConfiguration cfg, bool runServer)
    {
        s.AddHangfire((sp, h) => h.UsePostgreSqlStorage(o =>
            o.UseConnectionFactory(new DataSourceConnectionFactory(sp.GetRequiredService<NpgsqlDataSource>()))));
        if (runServer) s.AddHangfireServer();
        return s;
    }
}
