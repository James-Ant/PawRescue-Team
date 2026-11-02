using Microsoft.AspNetCore.Mvc;
using PawRescue.Application.Common;
using PawRescue.Domain.Common;

namespace PawRescue.Api.Middleware;

/// <summary>
/// Converts exceptions into RFC 7807 ProblemDetails JSON so the mobile app
/// always receives the same error shape: { title, status, detail }.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (status, title, detail) = ex switch
            {
                AppException app => (app.StatusCode, "Request failed", app.Message), // polymorphic StatusCode
                DomainException d => (StatusCodes.Status400BadRequest, "Business rule violated", d.Message),
                OperationCanceledException => (499, "Request cancelled", "The request was cancelled."),
                _ => (StatusCodes.Status500InternalServerError, "Server error", "Something went wrong. Please try again.")
            };

            if (status >= 500) _logger.LogError(ex, "Unhandled exception");
            else _logger.LogInformation("Handled {Type}: {Message}", ex.GetType().Name, ex.Message);

            if (context.Response.HasStarted) throw;

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            }, options: null, contentType: "application/problem+json");
        }
    }
}
