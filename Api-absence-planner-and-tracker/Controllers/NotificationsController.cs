using AbsencePlanner.Core.Interfaces;
using Api_absence_planner_and_tracker.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_absence_planner_and_tracker.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _svc;
    public NotificationsController(INotificationService svc) => _svc = svc;

[HttpGet] public async Task<IActionResult> GetAll() => Ok(await _svc.GetForUserAsync(User.GetUserId()));
    [HttpGet("unread-count")] public async Task<IActionResult> UnreadCount() => Ok(new { count = await _svc.GetUnreadCountAsync(User.GetUserId()) });
  [HttpPost("{id}/read")] public async Task<IActionResult> Read(string id) { await _svc.MarkReadAsync(id); return Ok(); }
    [HttpPost("mark-all-read")] public async Task<IActionResult> MarkAllRead() { await _svc.MarkAllReadAsync(User.GetUserId()); return Ok(); }
}
