using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _emailSettings = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(Email email)
    {
        try
        {
            _logger.LogInformation($"Sending email to {email.To} with subject {email.Subject} and body {email.Body}");

            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
            client.EnableSsl = true;

            await client.SendMailAsync(new MailMessage(_emailSettings.SenderEmail, email.To, email.Subject, email.Body));
            _logger.LogInformation($"Email sent to {email.To}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email to {email.To}");
            throw;
        }
    }
}