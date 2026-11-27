using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Orion.Application.Interfaces;
using Orion.Application.Services;

namespace Orion.Application;

/// <summary>
/// Registers application validators and use-case services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
