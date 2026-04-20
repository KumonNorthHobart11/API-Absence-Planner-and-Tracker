using System.Security.Claims;

namespace Api_absence_planner_and_tracker.Helpers;

public static class ClaimsHelper
{
    public static string GetUserId(this ClaimsPrincipal user) => user.FindFirstValue("userId") ?? "";
    public static string GetRole(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Role) ?? "";
 public static string GetName(this ClaimsPrincipal user) => user.FindFirstValue("name") ?? "";
    public static string GetEmail(this ClaimsPrincipal user) => user.FindFirstValue("email") ?? "";
}
