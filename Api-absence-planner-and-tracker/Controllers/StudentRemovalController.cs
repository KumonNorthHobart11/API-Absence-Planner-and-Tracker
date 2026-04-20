using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using Api_absence_planner_and_tracker.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_absence_planner_and_tracker.Controllers;

[ApiController]
[Route("api/student-removals")]
[Authorize]
public class StudentRemovalController : ControllerBase
{
    private readonly IStudentRemovalService _svc;
    public StudentRemovalController(IStudentRemovalService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        if (!string.IsNullOrEmpty(status))
            return Ok(await _svc.GetByStatusAsync(status, User.GetUserId(), User.GetRole()));
        return Ok(await _svc.GetAllAsync(User.GetUserId(), User.GetRole()));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRemovalRequest req) =>
        Created("", await _svc.CreateAsync(req, User.GetUserId(), User.GetName()));

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(string id) { await _svc.ApproveAsync(id, User.GetUserId()); return Ok(); }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(string id) { await _svc.RejectAsync(id, User.GetUserId()); return Ok(); }
}
