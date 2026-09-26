using System.Net;
using System.Net.Mail;

namespace Healthcare.Api.Services;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null);
}

public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
    {
        var host = config["Smtp:Host"];
        var user = config["Smtp:User"];
        var password = config["Smtp:Password"];
        var from = config["Smtp:From"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("DEV EMAIL => To: {To}; Subject: {Subject}; Body: {Body}", to, subject, body);
            return;
        }

        var port = int.TryParse(config["Smtp:Port"], out var configuredPort) ? configuredPort : 587;
        var enableSsl = !string.Equals(config["Smtp:EnableSsl"], "false", StringComparison.OrdinalIgnoreCase);
        password = password.Replace(" ", "");
        from = string.IsNullOrWhiteSpace(from) ? user : from;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(user, password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 30000
        };

        using var message = new MailMessage
        {
            From = new MailAddress(from),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(to);

        if (attachment is not null)
        {
            message.Attachments.Add(new Attachment(
                new MemoryStream(attachment), attachmentName ?? "document.pdf", "application/pdf"));
        }

        await client.SendMailAsync(message);
    }
}
