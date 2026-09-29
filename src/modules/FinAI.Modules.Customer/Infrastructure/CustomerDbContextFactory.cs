using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinAI.Modules.Customer.Infrastructure;

public sealed class CustomerDbContextFactory : IDesignTimeDbContextFactory<CustomerDbContext>
{
    public CustomerDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
                 ?? "Host=localhost;Database=finai;Username=finai;Password=finai";
        var options = new DbContextOptionsBuilder<CustomerDbContext>().UseNpgsql(cs).Options;
        return new CustomerDbContext(options);
    }
}
