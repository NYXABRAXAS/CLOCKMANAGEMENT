using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using STLMS.Application.Common.Interfaces;

namespace STLMS.Infrastructure.ExternalServices.Email;

public class SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var host = configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogWarning("Email not sent (SMTP not configured): \"{Subject}\" -> {ToEmail}", subject, toEmail);
            return false;
        }

        try
        {
            var message = new MimeMessage();
            var fromName = configuration["Smtp:FromName"] ?? "STLMS";
            var fromAddress = configuration["Smtp:FromAddress"] ?? "no-reply@stlms.local";
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            var port = configuration.GetValue<int?>("Smtp:Port") ?? 587;
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls, ct);

            var username = configuration["Smtp:Username"];
            if (!string.IsNullOrWhiteSpace(username))
            {
                await client.AuthenticateAsync(username, configuration["Smtp:Password"], ct);
            }

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email \"{Subject}\" to {ToEmail}", subject, toEmail);
            return false;
        }
    }
}
