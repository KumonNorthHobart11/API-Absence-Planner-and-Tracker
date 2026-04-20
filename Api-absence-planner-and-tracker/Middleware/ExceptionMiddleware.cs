using System.Net;
using System.Text.Json;

namespace Api_absence_planner_and_tracker.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next; _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      try { await _next(context); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
       var (code, msg) = ex switch
{
    KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
      ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
       InvalidOperationException => (HttpStatusCode.Conflict, ex.Message),
UnauthorizedAccessException => (HttpStatusCode.Forbidden, ex.Message),
      _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };
            context.Response.StatusCode = (int)code;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = msg }));
 }
 }
}
