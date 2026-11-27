using Orion.Api.Middlewares;
using Orion.Application;
using Orion.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendClient", policy =>
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Orion API v1");
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendClient");
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
