using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using Api_absence_planner_and_tracker.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_absence_planner_and_tracker.Controllers;

[ApiController]
[Route("api/students")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _svc;
    private readonly IUserService _userSvc;
    public StudentsController(IStudentService svc, IUserService userSvc) { _svc = svc; _userSvc = userSvc; }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync(User.GetUserId(), User.GetRole()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id) => Ok(await _svc.GetByIdAsync(id));

    [HttpGet("by-student-id/{studentId}")]
    public async Task<IActionResult> GetByStudentId(string studentId)
    {
        var s = await _svc.GetByStudentIdAsync(studentId);
        return s == null ? NotFound() : Ok(s);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentRequest req)
    {
        var u = await _userSvc.GetByIdAsync(User.GetUserId());
        var result = await _svc.CreateAsync(req, u.Id, u.Name, u.Email, u.Phone, u.Location, User.GetRole());
        return Created("", result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateStudentRequest req)
    {
        await _svc.UpdateAsync(id, req, User.GetRole());
        return Ok();
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id) { await _svc.DeleteAsync(id); return NoContent(); }

    [HttpPost("{id}/link")]
    public async Task<IActionResult> Link(string id, LinkStudentRequest req)
    {
        var u = await _userSvc.GetByIdAsync(User.GetUserId());
        await _svc.LinkAsync(id, u.Id, u.Name, u.Email, u.Phone, u.Location, req.Relation);
        return Ok();
    }
}
