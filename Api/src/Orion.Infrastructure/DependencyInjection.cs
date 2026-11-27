using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orion.Application.Interfaces;
using Orion.Domain.Interfaces;
using Orion.Infrastructure.Persistence;
using Orion.Infrastructure.Repositories;
using Orion.Infrastructure.Security;

namespace Orion.Infrastructure;

/// <summary>
/// Registers EF Core PostgreSQL persistence and infrastructure services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

        services.AddOptions<JwtSettings>()
            .Configure(options =>
            {
                var section = configuration.GetSection(JwtSettings.SectionName);

                var issuer = section[nameof(JwtSettings.Issuer)];
                if (!string.IsNullOrWhiteSpace(issuer))
                {
                    options.Issuer = issuer.Trim();
                }

                var audience = section[nameof(JwtSettings.Audience)];
                if (!string.IsNullOrWhiteSpace(audience))
                {
                    options.Audience = audience.Trim();
                }

                options.SecretKey = section[nameof(JwtSettings.SecretKey)];
                options.Key = section[nameof(JwtSettings.Key)];

                var expirationValue = section[nameof(JwtSettings.ExpirationMinutes)]
                    ?? section[nameof(JwtSettings.AccessTokenExpirationMinutes)];

                if (int.TryParse(expirationValue, out var expirationMinutes) &&
                    expirationMinutes > 0)
                {
                    options.ExpirationMinutes = expirationMinutes;
                }

                if (int.TryParse(section[nameof(JwtSettings.RememberMeExpirationDays)], out var rememberDays) &&
                    rememberDays > 0)
                {
                    options.RememberMeExpirationDays = rememberDays;
                }
            });

        services.AddOptions<GitHubOAuthSettings>()
            .Configure(options =>
            {
                var section = configuration.GetSection(GitHubOAuthSettings.SectionName);
                options.ClientId = section[nameof(GitHubOAuthSettings.ClientId)]?.Trim() ?? string.Empty;
                options.ClientSecret = section[nameof(GitHubOAuthSettings.ClientSecret)]?.Trim() ?? string.Empty;
                options.CallbackUrl = section[nameof(GitHubOAuthSettings.CallbackUrl)]?.Trim() ?? string.Empty;
                options.FrontendRedirectUrl = section[nameof(GitHubOAuthSettings.FrontendRedirectUrl)]?.Trim() ?? string.Empty;
            });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<JwtSigningKeyProvider>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
