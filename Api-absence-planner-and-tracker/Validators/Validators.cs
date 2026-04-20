using AbsencePlanner.Core.DTOs;
using FluentValidation;

namespace Api_absence_planner_and_tracker.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.Location).NotEmpty();
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Identifier).NotEmpty();
    }
}

public class VerifyLoginRequestValidator : AbstractValidator<VerifyLoginRequest>
{
    public VerifyLoginRequestValidator()
    {
        RuleFor(x => x.Identifier).NotEmpty();
        RuleFor(x => x.Otp).NotEmpty().Length(6).Matches(@"^\d{6}$")
       .WithMessage("OTP must be a 6-digit number.");
    }
}

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches(@"^\d{6}$")
    .WithMessage("Verification code must be a 6-digit number.");
    }
}

public class ResendOtpRequestValidator : AbstractValidator<ResendOtpRequest>
{
    public ResendOtpRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.Role).NotEmpty().Must(r => r is "superadmin" or "admin" or "user");
    }
}

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Grade).NotEmpty();
        RuleFor(x => x.Section).NotEmpty();
        RuleFor(x => x.Subjects).NotNull().NotEmpty()
   .WithMessage("At least one subject is required.");

        // ParentRelation required when user role creates the student (Users list is empty/null)
        RuleFor(x => x.ParentRelation)
             .NotEmpty()
             .Must(r => r is "father" or "mother" or "guardian")
             .WithMessage("ParentRelation must be 'father', 'mother', or 'guardian'.")
             .When(x => x.Users == null || x.Users.Count == 0);
    }
}

public class CreateHolidayRequestValidator : AbstractValidator<CreateHolidayRequest>
{
    public CreateHolidayRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x.SubmissionDeadline).NotEmpty();
    }
}

public class CreateAbsenceRequestValidator : AbstractValidator<CreateAbsenceRequest>
{
    public CreateAbsenceRequestValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
        RuleFor(x => x.HomeworkLoad).NotEmpty().Must(h => h is "Increase" or "Decrease" or "Moderate");
    }
}

public class CreateRemovalRequestValidator : AbstractValidator<CreateRemovalRequest>
{
    public CreateRemovalRequestValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}

public class LinkStudentRequestValidator : AbstractValidator<LinkStudentRequest>
{
    public LinkStudentRequestValidator()
    {
        RuleFor(x => x.Relation).NotEmpty().Must(r => r is "father" or "mother" or "guardian");
    }
}
