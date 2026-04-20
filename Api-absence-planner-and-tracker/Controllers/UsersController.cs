using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using Api_absence_planner_and_tracker.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_absence_planner_and_tracker.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "admin,superadmin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _svc;
    public UsersController(IUserService svc) => _svc = svc;

    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());
    [HttpGet("{id}")] public async Task<IActionResult> Get(string id) => Ok(await _svc.GetByIdAsync(id));
    [HttpPost] public async Task<IActionResult> Create(CreateUserRequest req) => Created("", await _svc.CreateAsync(req));
    [HttpPut("{id}")] public async Task<IActionResult> Update(string id, UpdateUserRequest req) { await _svc.UpdateAsync(id, req); return Ok(); }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(string id) { await _svc.DeleteAsync(id); return NoContent(); }

    /// <summary>
    /// SuperAdmin only — permanently deletes an admin account.
    /// </summary>
    [HttpDelete("{id}/admin")]
    [Authorize(Roles = "superadmin")]
    public async Task<IActionResult> DeleteAdmin(string id)
    {
        await _svc.DeleteAdminAsync(id, User.GetRole());
      return NoContent();
    }

    [HttpGet("pending-approval")] public async Task<IActionResult> Pending() => Ok(await _svc.GetPendingApprovalAsync());
    [HttpGet("rejected")] public async Task<IActionResult> Rejected() => Ok(await _svc.GetRejectedAsync());
    [HttpPost("{id}/approve")] public async Task<IActionResult> Approve(string id) { await _svc.ApproveAsync(id); return Ok(); }
    [HttpPost("{id}/reject")] public async Task<IActionResult> Reject(string id) { await _svc.RejectAsync(id); return Ok(); }
}
