using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using OficinaMecanica.Application.Abstractions;

namespace OficinaMecanica.Infrastructure.Notifications;

public sealed class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken)
    {
        var section = configuration.GetSection("Email");
        var host = section["Host"] ?? throw new InvalidOperationException("Email:Host não configurado.");
        var username = section["Username"] ?? throw new InvalidOperationException("Email:Username não configurado.");
        var password = section["Password"] ?? throw new InvalidOperationException("Email:Password não configurado.");
        var from = section["From"] ?? "no-reply@oficina.local";
        var port = section.GetValue("Port", 587);

        using var message = new MailMessage(from, recipient, subject, body) { IsBodyHtml = false };
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = section.GetValue("EnableSsl", true),
            Credentials = new NetworkCredential(username, password)
        };
        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }
}
