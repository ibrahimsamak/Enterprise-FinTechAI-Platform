using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinAI.Modules.Account.Infrastructure;

public sealed class AccountDbContextFactory : IDesignTimeDbContextFactory<AccountDbContext>
{
    public AccountDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
                 ?? "Host=localhost;Database=finai;Username=finai;Password=finai";
        var options = new DbContextOptionsBuilder<AccountDbContext>().UseNpgsql(cs).Options;
        return new AccountDbContext(options);
    }
}
