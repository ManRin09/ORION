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

                options.Key = section[nameof(JwtSettings.Key)];

                if (int.TryParse(section[nameof(JwtSettings.AccessTokenExpirationMinutes)], out var accessMinutes) &&
                    accessMinutes > 0)
                {
                    options.AccessTokenExpirationMinutes = accessMinutes;
                }

                if (int.TryParse(section[nameof(JwtSettings.RememberMeExpirationDays)], out var rememberDays) &&
                    rememberDays > 0)
                {
                    options.RememberMeExpirationDays = rememberDays;
                }
            });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<JwtSigningKeyProvider>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
