using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Orion.Domain.Exceptions;

namespace Orion.Api.Middlewares;

/// <summary>
/// Translates validation, domain, concurrency, and unexpected exceptions into RFC 9457 ProblemDetails responses.
/// </summary>
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                logger.LogWarning(
                    exception,
                    "Response has already started; skipping ProblemDetails handling for {Path}.",
                    context.Request.Path);
                throw;
            }

            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        var problemDetails = CreateProblemDetails(context, exception);
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var wroteResponse = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = problemDetails
        });

        if (!wroteResponse)
        {
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception) =>
        exception switch
        {
            ValidationException validationException => BuildValidationProblemDetails(context, validationException),

            InvalidCredentialsException invalidCredentials => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                Title = "Unauthorized",
                Detail = invalidCredentials.Message,
                Instance = context.Request.Path
            },

            UserNotFoundException notFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Title = "Resource not found",
                Detail = notFound.Message,
                Instance = context.Request.Path
            },

            EmailAlreadyExistsException or UsernameAlreadyExistsException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                Title = "Conflict",
                Detail = exception.Message,
                Instance = context.Request.Path
            },

            DbUpdateException dbUpdateException when IsUniqueConstraintViolation(dbUpdateException) => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                Title = "Conflict",
                Detail = "An adventurer with the same email or username already exists.",
                Instance = context.Request.Path
            },

            DomainException domainException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "Domain rule violation",
                Detail = domainException.Message,
                Instance = context.Request.Path
            },

            _ => BuildUnexpectedProblemDetails(context, exception)
        };

    private static HttpValidationProblemDetails BuildValidationProblemDetails(
        HttpContext context,
        ValidationException validationException)
    {
        var errors = validationException.Errors
            .GroupBy(error => ToCamelCase(error.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray(),
                StringComparer.Ordinal);

        return new HttpValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more validation errors occurred.",
            Detail = "Check the submitted adventurer attributes and cipher runes.",
            Instance = context.Request.Path
        };
    }

    private ProblemDetails BuildUnexpectedProblemDetails(HttpContext context, Exception exception)
    {
        logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}.",
            context.Request.Method,
            context.Request.Path);

        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Title = "An unexpected error occurred.",
            Detail = environment.IsDevelopment()
                ? exception.Message
                : "An unexpected internal error occurred. Please try again later.",
            Instance = context.Request.Path
        };
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException pgException &&
        pgException.SqlState == PostgresErrorCodes.UniqueViolation;

    private static string ToCamelCase(string propertyName) =>
        string.IsNullOrWhiteSpace(propertyName)
            ? "request"
            : JsonNamingPolicy.CamelCase.ConvertName(propertyName);
}
