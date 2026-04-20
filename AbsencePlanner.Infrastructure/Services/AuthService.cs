using System.Text.RegularExpressions;
using AbsencePlanner.Core.Configuration;
using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Core.Models;
using Microsoft.Extensions.Options;

namespace AbsencePlanner.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IFirestoreRepository _repo;
    private readonly IJwtService _jwt;
    private readonly INotificationService _notif;
    private readonly IOtpService _otp;
    private readonly IMessagingService _messaging;
    private readonly IOptionsMonitor<OtpSettings> _otpSettings;
    private const string Col = "users";

    public AuthService(IFirestoreRepository repo, IJwtService jwt, INotificationService notif, IOtpService otp, IMessagingService messaging, IOptionsMonitor<OtpSettings> otpSettings)
    {
        _repo = repo; _jwt = jwt; _notif = notif; _otp = otp; _messaging = messaging; _otpSettings = otpSettings;
    }

    private int OtpExpiryMinutes => _otpSettings.CurrentValue.ExpiryMinutes;
    private string SuperAdminOtp => _otpSettings.CurrentValue.SuperAdminOtp;

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest req)
    {
        if (!Regex.IsMatch(req.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Invalid email format.");
        if (string.IsNullOrWhiteSpace(req.Phone))
            throw new ArgumentException("Phone is required.");

        var users = await _repo.GetAllAsync<User>(Col);
        if (users.Any(u => u.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Email already in use.");
        if (users.Any(u => u.Phone == req.Phone))
            throw new InvalidOperationException("Phone already in use.");

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = req.Name,
            Email = req.Email,
            Phone = req.Phone,
            Location = req.Location,
            Role = "user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("990099"),
            Status = "pending_verification"
        };
        await _repo.SetAsync(Col, user.Id, user);

        // Generate OTP and send to both email and phone
        var code = await _otp.GenerateAndStoreAsync(user.Id, "registration");

        var emailBody = $@"
        <h2>Absence Planner - Email Verification</h2>
           <p>Hello {user.Name},</p>
            <p>Your verification code is: <strong>{code}</strong></p>
            <p>This code expires in {OtpExpiryMinutes} minutes.</p>";

        await _messaging.SendEmailAsync(user.Email, "Absence Planner - Verify Your Email", emailBody);

        try
        {
            await _messaging.SendSmsAsync(user.Phone, $"Absence Planner: Your verification code is {code}. Expires in {OtpExpiryMinutes} minutes.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }


        return new RegisterResponse(user.Id, "Verification code sent to your email and phone.");
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest req)
    {
        var user = await _repo.GetAsync<User>(Col, req.UserId) ?? throw new KeyNotFoundException("User not found.");
        if (user.Status != "pending_verification")
            throw new InvalidOperationException("User is not pending verification.");

        // Validate OTP
        var valid = await _otp.ValidateAsync(user.Id, req.Code, "registration");
        if (!valid) throw new ArgumentException("Invalid or expired verification code.");

        await _repo.UpdateFieldsAsync(Col, user.Id, new Dictionary<string, object?>
        {
            ["status"] = "pending_approval",
            ["emailVerified"] = true
        });

        await _notif.CreateForAdminsAsync($"New user registration pending approval: {user.Name} ({user.Email})", "registration_pending");
    }

    public async Task<ResendOtpResponse> ResendRegistrationOtpAsync(ResendOtpRequest req)
    {
        var user = await _repo.GetAsync<User>(Col, req.UserId) ?? throw new KeyNotFoundException("User not found.");
        if (user.Status != "pending_verification")
            throw new InvalidOperationException("User is not pending verification.");

        var code = await _otp.GenerateAndStoreAsync(user.Id, "registration");

        var emailBody = $@"
        <h2>Absence Planner - Email Verification</h2>
      <p>Hello {user.Name},</p>
        <p>Your new verification code is: <strong>{code}</strong></p>
        <p>This code expires in {OtpExpiryMinutes} minutes.</p>";

        await _messaging.SendEmailAsync(user.Email, "Absence Planner - New Verification Code", emailBody);
        await _messaging.SendSmsAsync(user.Phone, $"Absence Planner: Your new verification code is {code}. Expires in {OtpExpiryMinutes} minutes.");

        return new ResendOtpResponse("New verification code sent to your email and phone.");
    }

    public async Task<LoginInitResponse> LoginAsync(LoginRequest req)
    {
        var users = await _repo.GetAllAsync<User>(Col);
        var user = users.FirstOrDefault(u =>
            u.Email.Equals(req.Identifier, StringComparison.OrdinalIgnoreCase) || u.Phone == req.Identifier)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Status == "pending_verification") throw new InvalidOperationException("Please verify your email first.");
        if (user.Status == "pending_approval") throw new InvalidOperationException("Your account is awaiting admin approval.");
        if (user.Status == "rejected") throw new InvalidOperationException("Your registration was rejected.");

        // SuperAdmin uses a fixed OTP configured in appsettings — no email/SMS sent
        if (user.Role == "superadmin")
            return new LoginInitResponse(user.Id, "fixed", "Use your configured OTP to login.");

        // Generate OTP
        var code = await _otp.GenerateAndStoreAsync(user.Id, "login");

        // Determine channel: if identifier is email, send email; if phone, send SMS
        var isEmail = req.Identifier.Contains('@');
        if (isEmail)
        {
            var emailBody = $@"
<h2>Absence Planner - Login Verification</h2>
<p>Hello {user.Name},</p>
<p>Your login code is: <strong>{code}</strong></p>
<p>This code expires in {OtpExpiryMinutes} minutes.</p>";
            await _messaging.SendEmailAsync(user.Email, "Absence Planner - Login Code", emailBody);
        }
        else
        {
            await _messaging.SendSmsAsync(user.Phone, $"Absence Planner: Your login code is {code}. Expires in {OtpExpiryMinutes} minutes.");
        }

        var channel = isEmail ? "email" : "sms";
        return new LoginInitResponse(user.Id, channel, $"Verification code sent via {channel}.");
    }

    public async Task<LoginResponse> VerifyLoginAsync(VerifyLoginRequest req)
    {
        var users = await _repo.GetAllAsync<User>(Col);
        var user = users.FirstOrDefault(u =>
       u.Email.Equals(req.Identifier, StringComparison.OrdinalIgnoreCase) || u.Phone == req.Identifier)
         ?? throw new KeyNotFoundException("User not found.");

        // SuperAdmin: validate against fixed OTP from appsettings, no DB lookup
        if (user.Role == "superadmin")
        {
            if (req.Otp != SuperAdminOtp)
                throw new ArgumentException("Invalid OTP.");
            return new LoginResponse(_jwt.GenerateToken(user.Id, user.Role, user.Name, user.Email));
        }

        // All other users: validate and delete OTP from database
        var valid = await _otp.ValidateAsync(user.Id, req.Otp, "login");
        if (!valid) throw new ArgumentException("Invalid or expired OTP.");

        return new LoginResponse(_jwt.GenerateToken(user.Id, user.Role, user.Name, user.Email));
    }

    public Task<LoginResponse> RefreshAsync(string userId, string role, string name, string email)
    {
        return Task.FromResult(new LoginResponse(_jwt.GenerateToken(userId, role, name, email)));
    }
}
