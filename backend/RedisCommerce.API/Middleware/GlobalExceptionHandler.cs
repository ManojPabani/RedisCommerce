using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RedisCommerce.Domain.Exceptions;

namespace RedisCommerce.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            ProductNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            OrderNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            EmptyCartException => (StatusCodes.Status400BadRequest, "Empty Cart", exception.Message),
            InvalidOrderStateException => (StatusCodes.Status400BadRequest, "Invalid Order State", exception.Message),
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                string.Join(" ", validationException.Errors.Select(e => e.ErrorMessage))),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred."),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Title}", title);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
