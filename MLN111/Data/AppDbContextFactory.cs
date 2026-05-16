using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MLN111.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(BuildDesignTimeConnectionString())
            .Options;
        return new AppDbContext(opts);
    }

    private static string BuildDesignTimeConnectionString()
    {
        var env = Environment.GetEnvironmentVariable("MLN111_DESIGN_PG");
        if (!string.IsNullOrWhiteSpace(env))
            return env;

        try
        {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            return DatabaseConnection.Resolve(cfg);
        }
        catch (InvalidOperationException)
        {
            return "Host=127.0.0.1;Port=5432;Database=mln111_design;Username=postgres;Password=postgres;SSL Mode=Prefer";
        }
    }
}
