using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EstagioCheck.API.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger)
{
    public async Task SendResetCodeAsync(string toEmail, string code)
    {
        var smtpHost = config["Email:SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost não configurado.");
        var smtpPort = int.TryParse(config["Email:SmtpPort"], out var p) ? p : 587;
        var smtpUser = config["Email:Username"] ?? throw new InvalidOperationException("Email:Username não configurado.");
        var smtpPass = config["Email:Password"] ?? throw new InvalidOperationException("Email:Password não configurado.");
        var fromName = config["Email:FromName"] ?? "EstágioCheck UDF";
        var fromAddr = config["Email:FromAddress"] ?? smtpUser;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddr));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "EstágioCheck UDF – Código de recuperação de senha";

        message.Body = new TextPart("plain")
        {
            Text = $"Olá!\n\nSeu código de verificação é: {code}\n\nO código expira em 15 minutos. Se não foi você, ignore este e-mail.\n\nEstágioCheck UDF"
        };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }

        logger.LogInformation("Código de recuperação enviado para {Email}", toEmail);
    }
}
