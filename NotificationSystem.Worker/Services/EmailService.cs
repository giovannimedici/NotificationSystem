using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailSender _emailSender;
    private readonly EmailSettings _emailSettings;

    public EmailService(
        IOptions<EmailSettings> options,
        IEmailSender emailSender,
        ILogger<EmailService> logger)
    {
        _emailSettings = options.Value;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task SendEmailAsync(Email email)
    {
        try
        {
            _logger.LogInformation("Sending email to {To} with subject {Subject} and body {Body}", email.To, email.Subject, email.Body);

            await _emailSender.SendAsync(
                _emailSettings.SenderEmail,
                email.To,
                email.Subject,
                email.Body);

            _logger.LogInformation("Email sent to {To}", email.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", email.To);
            throw;
        }
    }
}
