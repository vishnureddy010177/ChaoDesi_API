namespace ChaoDesi.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string bodyHtml);
}