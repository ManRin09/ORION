using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Orion.Infrastructure.Persistence;

namespace Orion.Api.Persistence;

/// <summary>
/// Enables EF Core CLI migrations using Neon's direct (non-pooled) endpoint to avoid PgBouncer lock issues.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string DesignTimeFallbackConnection =
        "Host=localhost;Port=5432;Database=orion_design;Username=postgres;Password=postgres";

    public AppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var basePath = Directory.GetCurrentDirectory();
        var apiProjectPath = Path.Combine(basePath, "src", "Orion.Api");
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")) && Directory.Exists(apiProjectPath))
        {
            basePath = apiProjectPath;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("<NEON_", StringComparison.Ordinal))
        {
            connectionString = DesignTimeFallbackConnection;
        }
        else if (connectionString.Contains("-pooler.", StringComparison.OrdinalIgnoreCase))
        {
            // Neon EF Core migrations must run against the direct endpoint instead of the PgBouncer pooler.
            connectionString = connectionString.Replace("-pooler.", ".", StringComparison.OrdinalIgnoreCase);
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });

        return new AppDbContext(optionsBuilder.Options);
    }
}
