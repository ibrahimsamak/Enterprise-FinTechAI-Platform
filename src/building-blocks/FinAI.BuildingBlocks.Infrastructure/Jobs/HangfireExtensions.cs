using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinAI.BuildingBlocks.Infrastructure.Jobs;

public static class HangfireExtensions
{
    public static IServiceCollection AddFinAiHangfire(this IServiceCollection s, IConfiguration cfg, bool runServer)
    {
        s.AddHangfire(h => h.UsePostgreSqlStorage(o => o.UseNpgsqlConnection(cfg.GetConnectionString("Postgres"))));
        if (runServer) s.AddHangfireServer();
        return s;
    }
}
