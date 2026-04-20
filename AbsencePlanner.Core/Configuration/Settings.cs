namespace AbsencePlanner.Core.Configuration;

public class FirebaseSettings
{
    public string ProjectId { get; set; } = string.Empty;
    public string CredentialPath { get; set; } = string.Empty;
    public string DatabaseId { get; set; } = "(default)";
}

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Absence Planner";
}

public class TwilioSettings
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public class OtpSettings
{
    public int ExpiryMinutes { get; set; } = 10;
    public string SuperAdminOtp { get; set; } = "000000";
}

public class AppSettings
{
    public int SeedVersion { get; set; } = 1;
}
