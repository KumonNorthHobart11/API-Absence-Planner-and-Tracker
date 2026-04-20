using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using Api_absence_planner_and_tracker.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_absence_planner_and_tracker.Controllers;

[ApiController]
[Route("api/absences")]
[Authorize]
public class AbsencesController : ControllerBase
{
    private readonly IAbsenceService _svc;
    public AbsencesController(IAbsenceService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
  {
    if (!string.IsNullOrEmpty(status))
       return Ok(await _svc.GetByStatusAsync(status, User.GetUserId(), User.GetRole()));
        return Ok(await _svc.GetAllAsync(User.GetUserId(), User.GetRole()));
  }

    [HttpGet("{id}")] public async Task<IActionResult> Get(string id) => Ok(await _svc.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateAbsenceRequest req) =>
  Created("", await _svc.CreateAsync(req, User.GetUserId(), User.GetName()));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateAbsenceRequest req)
    {
        await _svc.UpdateAsync(id, req, User.GetUserId());
        return Ok();
    }

    [HttpPost("{id}/lock")] public async Task<IActionResult> Lock(string id) { await _svc.LockAsync(id, User.GetUserId()); return Ok(); }
    [HttpPost("{id}/unlock")] public async Task<IActionResult> Unlock(string id) { await _svc.UnlockAsync(id); return Ok(); }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("{id}/approve")] public async Task<IActionResult> Approve(string id) { await _svc.ApproveAsync(id); return Ok(); }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("{id}/reject")] public async Task<IActionResult> Reject(string id) { await _svc.RejectAsync(id); return Ok(); }

    [HttpGet("expired")] public async Task<IActionResult> Expired() => Ok(await _svc.GetExpiredAsync(User.GetUserId()));
}
