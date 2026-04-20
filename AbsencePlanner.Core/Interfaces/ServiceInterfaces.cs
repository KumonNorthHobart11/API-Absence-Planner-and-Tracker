using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Models;

namespace AbsencePlanner.Core.Interfaces;

public interface IJwtService
{
    string GenerateToken(string userId, string role, string name, string email);
}

public interface IOtpService
{
    Task<string> GenerateAndStoreAsync(string userId, string purpose);
    Task<bool> ValidateAsync(string userId, string code, string purpose);
    Task CleanupExpiredAsync(string userId);
}

public interface IMessagingService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendSmsAsync(string toPhone, string message);
}

public interface INotificationService
{
    Task CreateAsync(string userId, string message, string type);
    Task CreateForAdminsAsync(string message, string type);
    Task<List<AppNotification>> GetForUserAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkReadAsync(string notificationId);
    Task MarkAllReadAsync(string userId);
}

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest req);
    Task VerifyEmailAsync(VerifyEmailRequest req);
    Task<ResendOtpResponse> ResendRegistrationOtpAsync(ResendOtpRequest req);
    Task<LoginInitResponse> LoginAsync(LoginRequest req);
    Task<LoginResponse> VerifyLoginAsync(VerifyLoginRequest req);
    Task<LoginResponse> RefreshAsync(string userId, string role, string name, string email);
}

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User> GetByIdAsync(string id);
    Task<User> CreateAsync(CreateUserRequest req);
    Task UpdateAsync(string id, UpdateUserRequest req);
    Task DeleteAsync(string id);
    Task DeleteAdminAsync(string targetId, string callerRole);
    Task<List<User>> GetPendingApprovalAsync();
    Task<List<User>> GetRejectedAsync();
    Task ApproveAsync(string id);
    Task RejectAsync(string id);
}

public interface IStudentService
{
    Task<List<Student>> GetAllAsync(string userId, string role);
    Task<Student> GetByIdAsync(string id);
    Task<Student?> GetByStudentIdAsync(string studentId);
    Task<Student> CreateAsync(CreateStudentRequest req, string userId, string userName, string userEmail, string userPhone, string userLocation, string role);
    Task UpdateAsync(string id, UpdateStudentRequest req, string role);
    Task DeleteAsync(string id);
    Task LinkAsync(string id, string userId, string userName, string userEmail, string userPhone, string userLocation, string relation);
}

public interface IHolidayService
{
    Task<List<Holiday>> GetAllAsync();
    Task<Holiday> GetByIdAsync(string id);
    Task<Holiday> CreateAsync(CreateHolidayRequest req, string userId);
    Task UpdateAsync(string id, UpdateHolidayRequest req);
    Task DeleteAsync(string id);
}

public interface IAbsenceService
{
    Task<List<Absence>> GetAllAsync(string userId, string role);
    Task<Absence> GetByIdAsync(string id);
    Task<List<Absence>> GetByStatusAsync(string status, string userId, string role);
    Task<Absence> CreateAsync(CreateAbsenceRequest req, string userId, string userName);
    Task UpdateAsync(string id, UpdateAbsenceRequest req, string userId);
    Task LockAsync(string id, string userId);
    Task UnlockAsync(string id);
    Task ApproveAsync(string id);
    Task RejectAsync(string id);
    Task<List<ExpiredSubmissionDto>> GetExpiredAsync(string userId);
}

public interface IPermissionService
{
    Task<List<MenuPermission>> GetMenuPermissionsAsync();
    Task SaveMenuPermissionsAsync(List<MenuPermissionDto> perms, string callerRole);
    Task<List<FeaturePermission>> GetFeaturePermissionsAsync(string? role);
    Task SaveFeaturePermissionsAsync(List<FeaturePermissionDto> perms, string callerRole);
    Task<CalendarDayConfig> GetCalendarDaysAsync();
    Task SaveCalendarDaysAsync(CalendarDaysRequest req);
    Task<bool> CheckMenuAsync(string menuKey, string role);
    Task<bool> CheckFeatureAsync(string menuKey, string role, string action);
}

public interface IStudentRemovalService
{
    Task<List<StudentRemovalRequest>> GetAllAsync(string userId, string role);
    Task<List<StudentRemovalRequest>> GetByStatusAsync(string status, string userId, string role);
    Task<StudentRemovalRequest> CreateAsync(CreateRemovalRequest req, string userId, string userName);
    Task ApproveAsync(string id, string adminUserId);
    Task RejectAsync(string id, string adminUserId);
}

public interface ITokenRevocationService
{
    Task RevokeAsync(string userId);
    Task<bool> IsRevokedAsync(string userId);
}
