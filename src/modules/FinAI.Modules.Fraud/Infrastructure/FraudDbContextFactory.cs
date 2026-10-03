using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinAI.Modules.Fraud.Infrastructure;

public sealed class FraudDbContextFactory : IDesignTimeDbContextFactory<FraudDbContext>
{
    public FraudDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
                 ?? "Host=localhost;Database=finai;Username=finai;Password=finai";
        var options = new DbContextOptionsBuilder<FraudDbContext>().UseNpgsql(cs).Options;
        return new FraudDbContext(options);
    }
}
