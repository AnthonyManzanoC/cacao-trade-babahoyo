using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using OrigenCacao.Application;

namespace OrigenCacao.Infrastructure;

public sealed class EmailService(AppDbContext db) : IEmailService
{
    public async Task SendReceiptAsync(string recipient, string subject, string body, byte[] content,
        string fileName, CancellationToken ct)
    {
        if (!System.Net.Mail.MailAddress.TryCreate(recipient, out _))
            throw new ArgumentException("El correo de destino no es válido.");
        var settings = await db.BusinessSettings.AsNoTracking().SingleAsync(x => x.Id == 1, ct);
        if (!settings.EmailSendingEnabled)
            throw new InvalidOperationException("El envío de correos está deshabilitado en Configuración.");
        if (string.IsNullOrWhiteSpace(settings.SmtpHost) || string.IsNullOrWhiteSpace(settings.SmtpEmail) ||
            string.IsNullOrWhiteSpace(settings.SmtpPassword))
            throw new InvalidOperationException("La configuración SMTP está incompleta.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.BusinessName, settings.SmtpEmail));
        message.To.Add(MailboxAddress.Parse(recipient.Trim()));
        message.Subject = subject;
        var builder = new BodyBuilder
        {
            HtmlBody = $"<div style=\"font-family:Arial,sans-serif;color:#263126\"><h2>{System.Net.WebUtility.HtmlEncode(settings.BusinessName)}</h2><p>{System.Net.WebUtility.HtmlEncode(body)}</p><p>El comprobante PDF está adjunto a este mensaje.</p></div>",
            TextBody = $"{settings.BusinessName}\n\n{body}\n\nEl comprobante PDF está adjunto."
        };
        builder.Attachments.Add(fileName, content, ContentType.Parse("application/pdf"));
        message.Body = builder.ToMessageBody();

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort,
            settings.SmtpUseSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None, ct);
        await client.AuthenticateAsync(settings.SmtpEmail, settings.SmtpPassword, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
