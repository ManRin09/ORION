using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Orion.Api.Middlewares;
using Orion.Application;
using Orion.Application.Interfaces;
using Orion.Domain.Entities;
using Orion.Domain.Enums;
using Orion.Domain.Interfaces;
using Orion.Infrastructure;
using Orion.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

var portValue = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(portValue, out var port) && port > 0)
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>, JwtSigningKeyProvider>((options, jwtOptions, keyProvider) =>
    {
        var settings = jwtOptions.Value;
        options.RequireHttpsMetadata = false;
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = keyProvider.SigningKey,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                    Title = "Unauthorized",
                    Detail = "A valid Orion JWT Bearer token is required to access this resource.",
                    Instance = context.Request.Path
                };

                await context.Response.WriteAsJsonAsync(problem);
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    Title = "Forbidden",
                    Detail = "You do not have sufficient guild permissions to perform this operation.",
                    Instance = context.Request.Path
                };

                await context.Response.WriteAsJsonAsync(problem);
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();

var allowedOrigins = ResolveAllowedOrigins(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendClient", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        const string schemeName = "Bearer";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[schemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste your Orion JWT access token (without the 'Bearer ' prefix)."
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(schemeName, document)] = []
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.TryAdd("X-Permitted-Cross-Domain-Policies", "none");
    await next();
});

app.UseMiddleware<GlobalExceptionMiddleware>();

var enableSwagger = app.Environment.IsDevelopment()
    || (bool.TryParse(app.Configuration["ENABLE_SWAGGER"], out var isSwaggerEnabled) && isSwaggerEnabled);

if (enableSwagger)
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Orion API v1");
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithTags("Health")
    .WithSummary("Service health check");

await SeedDefaultAdminAsync(app.Services);

app.Run();

static async Task SeedDefaultAdminAsync(IServiceProvider services)
{
    const string adminEmail = "juanjosemn160@gmail.com";
    const string adminUsername = "juanjo2409";
    const string adminFirstName = "Juan";
    const string adminMiddleName = "Maldonado";
    const string adminDefaultPassword = "Juan0811@";

    using var scope = services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

    try
    {
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var existingAdmin = await userRepository.GetByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            if (existingAdmin.Role != UserRole.Admin)
            {
                existingAdmin.ChangeRole(UserRole.Admin);
                await userRepository.SaveChangesAsync();
                logger.LogInformation("Promoted adventurer {Email} to Admin role.", adminEmail);
            }

            return;
        }

        if (await userRepository.ExistsByEmailAsync(adminEmail))
        {
            return;
        }

        var username = await userRepository.ExistsByUsernameAsync(adminUsername)
            ? $"{adminUsername}_admin"
            : adminUsername;

        var utcNow = DateTime.UtcNow;
        var adminUser = User.CreateLocal(
            firstName: adminFirstName,
            middleName: adminMiddleName,
            email: adminEmail,
            username: username,
            passwordHash: passwordHasher.Hash(adminDefaultPassword),
            acceptedTermsAtUtc: utcNow,
            utcNow: utcNow);

        adminUser.ChangeRole(UserRole.Admin, utcNow);

        await userRepository.AddAsync(adminUser);
        await userRepository.SaveChangesAsync();
        logger.LogInformation("Seeded default Admin adventurer {Email} (@{Username}).", adminEmail, username);
    }
    catch (Exception exception)
    {
        logger.LogWarning(exception, "Skipped default Admin seed check on startup.");
    }
}

static string[] ResolveAllowedOrigins(IConfiguration configuration)
{
    var origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "http://localhost:4200",
        "https://localhost:4200"
    };

    var configuredArray = configuration.GetSection("AllowedOrigins").Get<string[]>();
    if (configuredArray is not null)
    {
        foreach (var origin in configuredArray)
        {
            AddNormalizedOrigin(origins, origin);
        }
    }

    var configuredScalar = configuration["AllowedOrigins"];
    if (!string.IsNullOrWhiteSpace(configuredScalar))
    {
        foreach (var origin in configuredScalar.Split(
            [',', ';'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            AddNormalizedOrigin(origins, origin);
        }
    }

    return origins.ToArray();
}

static void AddNormalizedOrigin(HashSet<string> origins, string? origin)
{
    if (string.IsNullOrWhiteSpace(origin))
    {
        return;
    }

    var normalized = origin.Trim().TrimEnd('/');
    if (!string.IsNullOrWhiteSpace(normalized))
    {
        origins.Add(normalized);
    }
}

public partial class Program;
