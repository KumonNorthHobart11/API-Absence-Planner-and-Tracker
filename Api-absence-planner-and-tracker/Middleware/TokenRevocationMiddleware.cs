using System.Net;
using System.Text.Json;
using AbsencePlanner.Core.Interfaces;
using Api_absence_planner_and_tracker.Helpers;

namespace Api_absence_planner_and_tracker.Middleware;

public class TokenRevocationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenRevocationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITokenRevocationService revocationService)
  {
   // Only check authenticated requests that carry a userId claim
     if (context.User.Identity?.IsAuthenticated == true)
 {
            var userId = context.User.GetUserId();

   if (!string.IsNullOrEmpty(userId) && await revocationService.IsRevokedAsync(userId))
           {
      context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
      context.Response.ContentType = "application/json";
       await context.Response.WriteAsync(JsonSerializer.Serialize(new
      {
    error = "Your account has been removed. Please contact the administrator."
  }));
          return;
        }
     }

 await _next(context);
    }
}
