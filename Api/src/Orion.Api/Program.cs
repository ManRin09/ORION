using Orion.Api.Middlewares;
using Orion.Application;
using Orion.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var portValue = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(portValue, out var port) && port > 0)
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
builder.Services.AddOpenApi();

var app = builder.Build();

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
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithTags("Health")
    .WithSummary("Service health check");

app.Run();

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
