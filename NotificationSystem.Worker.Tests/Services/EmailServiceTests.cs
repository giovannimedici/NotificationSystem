using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace NotificationSystem.Worker.Tests.Services;

public class EmailServiceTests
{
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly Mock<ILogger<EmailService>> _loggerMock = new();
    private readonly EmailSettings _emailSettings = new()
    {
        SenderEmail = "sender@test.com",
        SenderPassword = "password",
        SmtpHost = "smtp.test.com",
        SmtpPort = 587
    };

    private EmailService CreateSut() =>
        new(Options.Create(_emailSettings), _emailSenderMock.Object, _loggerMock.Object);

    [Fact]
    public async Task SendEmailAsync_WhenSuccessful_SendsEmailWithCorrectParameters()
    {
        var email = new Email("recipient@test.com", "Welcome", "Hello there");
        var sut = CreateSut();

        await sut.SendEmailAsync(email);

        _emailSenderMock.Verify(
            s => s.SendAsync(
                _emailSettings.SenderEmail,
                email.To,
                email.Subject,
                email.Body),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WhenEmailSenderThrows_RethrowsException()
    {
        var email = new Email("recipient@test.com", "Welcome", "Hello there");
        var expectedException = new InvalidOperationException("SMTP unavailable");
        _emailSenderMock
            .Setup(s => s.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(expectedException);
        var sut = CreateSut();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.SendEmailAsync(email));

        Assert.Same(expectedException, exception);
        _emailSenderMock.Verify(
            s => s.SendAsync(
                _emailSettings.SenderEmail,
                email.To,
                email.Subject,
                email.Body),
            Times.Once);
    }
}
