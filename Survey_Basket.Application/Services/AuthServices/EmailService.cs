using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using Survey_Basket.Application.Settings;

namespace Survey_Basket.Application.Services.AuthServices;

public class EmailService(IOptions<MailSettings> mailSettings) : IEmailSender
{
    private readonly MailSettings _mailSettings = mailSettings.Value;
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var emailMessage = new MimeMessage
        {
            Sender = MailboxAddress.Parse(_mailSettings.Mail),
            Subject = subject
        };

        emailMessage.To.Add(MailboxAddress.Parse(email));

        var builder = new BodyBuilder { HtmlBody = htmlMessage };
        emailMessage.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        try
        {
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(emailMessage);
        }
        catch
        {
            throw;
        }
        finally
        {
            await smtp.DisconnectAsync(true);
            smtp.Dispose();
        }
    }
}
