using GuestService.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace GuestService.Presentation.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var response = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = "An error occurred while processing your request",
            Details = exception.Message
        };

        switch (exception)
        {
            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                response.StatusCode = (int)statusCode;
                response.Message = "Resource not found";
                response.Details = notFoundException.Message;
                break;

            case ValidationException validationException:
                statusCode = HttpStatusCode.UnprocessableEntity;
                response.StatusCode = (int)statusCode;
                response.Message = "Validation failed";
                response.Details = validationException.Message;
                response.Errors = validationException.Errors;
                break;

            case ArgumentException argumentException:
                statusCode = HttpStatusCode.BadRequest;
                response.StatusCode = (int)statusCode;
                response.Message = "Invalid argument";
                response.Details = argumentException.Message;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; set; }
}