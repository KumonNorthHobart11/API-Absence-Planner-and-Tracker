using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using Api_absence_planner_and_tracker.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_absence_planner_and_tracker.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize(Roles = "admin,superadmin")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _svc;
    public PermissionsController(IPermissionService svc) => _svc = svc;

    [HttpGet("menu")] public async Task<IActionResult> GetMenu() => Ok(await _svc.GetMenuPermissionsAsync());
    [HttpPut("menu")] public async Task<IActionResult> SaveMenu(List<MenuPermissionDto> perms) { await _svc.SaveMenuPermissionsAsync(perms, User.GetRole()); return Ok(); }

    [HttpGet("features")] public async Task<IActionResult> GetFeatures([FromQuery] string? role) => Ok(await _svc.GetFeaturePermissionsAsync(role));
    [HttpPut("features")] public async Task<IActionResult> SaveFeatures(List<FeaturePermissionDto> perms) { await _svc.SaveFeaturePermissionsAsync(perms, User.GetRole()); return Ok(); }

    [AllowAnonymous]
    [HttpGet("calendar-days")] public async Task<IActionResult> GetCalendarDays() => Ok(await _svc.GetCalendarDaysAsync());
    [HttpPut("calendar-days")] public async Task<IActionResult> SaveCalendarDays(CalendarDaysRequest req) { await _svc.SaveCalendarDaysAsync(req); return Ok(); }

    [AllowAnonymous]
    [HttpGet("check-menu")] public async Task<IActionResult> CheckMenu([FromQuery] string menuKey, [FromQuery] string role) => Ok(new { allowed = await _svc.CheckMenuAsync(menuKey, role) });

    [AllowAnonymous]
    [HttpGet("check-feature")] public async Task<IActionResult> CheckFeature([FromQuery] string menuKey, [FromQuery] string role, [FromQuery] string action) => Ok(new { allowed = await _svc.CheckFeatureAsync(menuKey, role, action) });
}
