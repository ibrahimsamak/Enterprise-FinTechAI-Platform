using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pgvector.EntityFrameworkCore;

namespace FinAI.AI.Rag;

public sealed class RagDbContextFactory : IDesignTimeDbContextFactory<RagDbContext>
{
    public RagDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
                 ?? "Host=localhost;Database=finai;Username=finai;Password=finai";
        var options = new DbContextOptionsBuilder<RagDbContext>()
            .UseNpgsql(cs, npg => npg.UseVector())
            .Options;
        return new RagDbContext(options);
    }
}
