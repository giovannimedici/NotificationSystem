using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public SmtpEmailSender(IOptions<EmailSettings> options)
    {
        _emailSettings = options.Value;
    }

    public async Task SendAsync(string from, string to, string subject, string body)
    {
        using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort);
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
        client.EnableSsl = true;

        await client.SendMailAsync(new MailMessage(from, to, subject, body));
    }
}
