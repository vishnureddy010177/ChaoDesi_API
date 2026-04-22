using ChaoDesi.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ChaoDesi.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            email.Body = new BodyBuilder
            {
                HtmlBody = bodyHtml
            }.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            // Important for strict SMTP servers
            //smtp.LocalDomain = "profiledm.com";

            await smtp.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {

        }
       
    }
}