using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using Api_absence_planner_and_tracker.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_absence_planner_and_tracker.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>
    /// Register a new user. Sends 6-digit OTP to email and phone.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var result = await _auth.RegisterAsync(req);
        return Created("", result);
    }

    /// <summary>
    /// Verify registration OTP. Transitions user to pending_approval.
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest req)
    {
        await _auth.VerifyEmailAsync(req);
        return Ok(new { message = "Email verified. Awaiting admin approval." });
    }

    /// <summary>
    /// Resend registration OTP if the previous one expired.
    /// </summary>
    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(ResendOtpRequest req) => Ok(await _auth.ResendRegistrationOtpAsync(req));

    /// <summary>
    /// Step 1: Initiate login. Sends 6-digit OTP via email or SMS based on identifier.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req) => Ok(await _auth.LoginAsync(req));

    /// <summary>
    /// Step 2: Verify login OTP. Returns JWT token.
    /// </summary>
    [HttpPost("verify-login")]
    public async Task<IActionResult> VerifyLogin(VerifyLoginRequest req) => Ok(await _auth.VerifyLoginAsync(req));

    [Authorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var result = await _auth.RefreshAsync(User.GetUserId(), User.GetRole(), User.GetName(), User.GetEmail());
        return Ok(result);
    }
}
