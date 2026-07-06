using System.Net;
using System.Text.Json;

namespace OrderFlow.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate               _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment           _env;

    public GlobalExceptionMiddleware(RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleAsync(context, ex);
        }
    }

    private Task HandleAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            KeyNotFoundException   => (HttpStatusCode.NotFound,           "The requested resource was not found."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,  "Access denied."),
            ArgumentException      => (HttpStatusCode.BadRequest,         "Invalid request."),
            _                      => (HttpStatusCode.InternalServerError,"An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var body = new
        {
            message,
            detail    = _env.IsDevelopment() ? ex.Message : null,
            traceId   = context.TraceIdentifier,
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}
