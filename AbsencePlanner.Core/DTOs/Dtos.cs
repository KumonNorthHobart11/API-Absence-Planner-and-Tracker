namespace AbsencePlanner.Core.DTOs;

// Auth
public record RegisterRequest(string Name, string Email, string Phone, string Location);
public record RegisterResponse(string UserId, string Message);
public record VerifyEmailRequest(string UserId, string Code);
public record ResendOtpRequest(string UserId);
public record ResendOtpResponse(string Message);
public record LoginRequest(string Identifier);
public record LoginInitResponse(string UserId, string Channel, string Message);
public record VerifyLoginRequest(string Identifier, string Otp);
public record LoginResponse(string Token);

// Users
public record CreateUserRequest(string Name, string Email, string Phone, string Location, string Role);
public record UpdateUserRequest(string Name, string Email, string Phone, string Location, string Role);

// Students
public record CreateStudentRequest(string Name, string Grade, string Section, List<SubjectDto> Subjects, string? ParentRelation, List<ParentEntryDto>? Users);
public record UpdateStudentRequest(string Name, string Grade, string Section, List<SubjectDto> Subjects, string? ParentRelation, List<ParentEntryDto>? Users);
public record LinkStudentRequest(string Relation);
public record SubjectDto(string Name, List<ScheduleDto> Schedules);
public record ScheduleDto(string Day, string StartTime, string EndTime);
public record ParentEntryDto(string Name, string Email, string Phone, string Location, string Relation);

// Holidays
public record CreateHolidayRequest(string Name, string StartDate, string EndDate, string SubmissionDeadline, string Description);
public record UpdateHolidayRequest(string Name, string StartDate, string EndDate, string SubmissionDeadline, string Description);

// Absences
public record CreateAbsenceRequest(string StudentId, string? HolidayId, string StartDate, string EndDate, string Reason, string HomeworkLoad, bool DigitalKumon);
public record UpdateAbsenceRequest(string StartDate, string EndDate, string Reason);

// Permissions
public record MenuPermissionDto(string MenuKey, string Label, List<string> Roles);
public record FeaturePermissionDto(string MenuKey, string Role, bool CanAdd, bool CanEdit, bool CanDelete, bool CanView);
public record CalendarDaysRequest(List<string> AllowedDays);

// Student Removal
public record CreateRemovalRequest(string StudentId, string Reason);

// Expired absence result
public record ExpiredSubmissionDto(string HolidayId, string HolidayName, string StudentId, string StudentName);
