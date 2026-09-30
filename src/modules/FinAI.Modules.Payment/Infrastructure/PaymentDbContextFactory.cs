using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinAI.Modules.Payment.Infrastructure;

public sealed class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
                 ?? "Host=localhost;Database=finai;Username=finai;Password=finai";
        var options = new DbContextOptionsBuilder<PaymentDbContext>().UseNpgsql(cs).Options;
        return new PaymentDbContext(options);
    }
}
