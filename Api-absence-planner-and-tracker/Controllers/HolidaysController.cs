using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using Api_absence_planner_and_tracker.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_absence_planner_and_tracker.Controllers;

[ApiController]
[Route("api/holidays")]
[Authorize]
public class HolidaysController : ControllerBase
{
    private readonly IHolidayService _svc;
    public HolidaysController(IHolidayService svc) => _svc = svc;

    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());
    [HttpGet("{id}")] public async Task<IActionResult> Get(string id) => Ok(await _svc.GetByIdAsync(id));

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost] public async Task<IActionResult> Create(CreateHolidayRequest req) => Created("", await _svc.CreateAsync(req, User.GetUserId()));

    [Authorize(Roles = "admin,superadmin")]
    [HttpPut("{id}")] public async Task<IActionResult> Update(string id, UpdateHolidayRequest req) { await _svc.UpdateAsync(id, req); return Ok(); }

    [Authorize(Roles = "admin,superadmin")]
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(string id) { await _svc.DeleteAsync(id); return NoContent(); }
}
