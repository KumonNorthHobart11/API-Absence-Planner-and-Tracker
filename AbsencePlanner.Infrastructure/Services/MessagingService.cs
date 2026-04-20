using AbsencePlanner.Core.Configuration;
using AbsencePlanner.Core.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AbsencePlanner.Infrastructure.Services;

public class MessagingService : IMessagingService
{
    private readonly IOptionsMonitor<SmtpSettings> _smtp;
    private readonly IOptionsMonitor<TwilioSettings> _twilio;
    private readonly ILogger<MessagingService> _logger;
    private string? _lastTwilioSid;

    public MessagingService(IOptionsMonitor<SmtpSettings> smtp, IOptionsMonitor<TwilioSettings> twilio, ILogger<MessagingService> logger)
    {
        _smtp = smtp;
        _twilio = twilio;
        _logger = logger;
    }

    private void EnsureTwilioInitialized()
    {
        var cfg = _twilio.CurrentValue;
        if (!string.IsNullOrEmpty(cfg.AccountSid) && cfg.AccountSid != _lastTwilioSid)
        {
            TwilioClient.Init(cfg.AccountSid, cfg.AuthToken);
            _lastTwilioSid = cfg.AccountSid;
        }
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var cfg = _smtp.CurrentValue;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(cfg.FromName, cfg.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(cfg.Host, cfg.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(cfg.Username, cfg.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendSmsAsync(string toPhone, string message)
    {
        try
        {
            EnsureTwilioInitialized();
            var cfg = _twilio.CurrentValue;

            // Ensure Australian format: if starts with 0, convert to +61
            var formattedPhone = toPhone.Trim();
            if (formattedPhone.StartsWith("0"))
                formattedPhone = "+61" + formattedPhone[1..];
            else if (!formattedPhone.StartsWith("+"))
                formattedPhone = "+61" + formattedPhone;

            var result = await MessageResource.CreateAsync(
                to: new PhoneNumber(formattedPhone),
                from: new PhoneNumber(cfg.FromNumber),
                body: message);

            _logger.LogInformation("SMS sent to {Phone}, SID: {Sid}", formattedPhone, result.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Phone}", toPhone);
            throw;
        }
    }
}
