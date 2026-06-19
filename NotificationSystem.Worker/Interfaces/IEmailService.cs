public interface IEmailService{
    Task SendEmailAsync(Email email);
}