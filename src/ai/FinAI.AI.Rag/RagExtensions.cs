using FinAI.AI.Rag.Ingestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinAI.AI.Rag;

public static class RagExtensions
{
    public static IServiceCollection AddFinAiRag(this IServiceCollection s, IConfiguration cfg)
    {
        s.AddDbContext<RagDbContext>(o => o.UseNpgsql(cfg.GetConnectionString("Postgres"), npg => npg.UseVector()));
        s.AddScoped<RagService>();
        s.AddScoped<RagIngestionService>();
        
        return s;
    }
}
