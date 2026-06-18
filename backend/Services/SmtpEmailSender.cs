using System.Net;
using System.Net.Mail;

namespace backend.Services
{
    public interface IEmailSender
    {
        Task SendPasswordResetEmailAsync(string toEmail, string recipientName, string resetLink);
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string recipientName, string resetLink)
        {
            var host = GetSetting("SMTP__HOST", "Smtp:Host", "smtp.gmail.com");
            var port = GetIntSetting("SMTP__PORT", "Smtp:Port", 587);
            var username = GetRequiredSetting("SMTP__USERNAME", "Smtp:Username");
            var password = GetRequiredSetting("SMTP__PASSWORD", "Smtp:Password");
            var fromAddress = GetSetting("SMTP__FROM", "Smtp:From", username);
            var fromName = GetSetting("SMTP__FROM_NAME", "Smtp:FromName", "BotForge");

            using var message = new MailMessage();
            message.From = new MailAddress(fromAddress, fromName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = "Recupera tu contraseña";
            message.IsBodyHtml = true;
            message.Body = $@"
                <div style='font-family:Segoe UI,Tahoma,sans-serif;color:#0f172a;'>
                    <h2 style='margin-bottom:12px;'>Restablecer contraseña</h2>
                    <p>Hola {WebUtility.HtmlEncode(recipientName)},</p>
                    <p>Recibimos una solicitud para restablecer tu contraseña.</p>
                    <p>
                        <a href='{WebUtility.HtmlEncode(resetLink)}'
                           style='display:inline-block;padding:12px 18px;background:#2563eb;color:#ffffff;text-decoration:none;border-radius:10px;font-weight:600;'>
                           Cambiar contraseña
                        </a>
                    </p>
                    <p>Si el botón no funciona, usa este enlace:</p>
                    <p>{WebUtility.HtmlEncode(resetLink)}</p>
                    <p>Este enlace expira en 60 minutos.</p>
                </div>";

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }

        private string GetRequiredSetting(string environmentKey, string configKey)
        {
            var value = GetSetting(environmentKey, configKey, string.Empty);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Missing SMTP configuration: {environmentKey} or {configKey}");
            }

            return value;
        }

        private string GetSetting(string environmentKey, string configKey, string fallback)
        {
            var environmentValue = Environment.GetEnvironmentVariable(environmentKey);
            if (!string.IsNullOrWhiteSpace(environmentValue))
            {
                return environmentValue;
            }

            var configuredValue = _configuration[configKey];
            return string.IsNullOrWhiteSpace(configuredValue) ? fallback : configuredValue;
        }

        private int GetIntSetting(string environmentKey, string configKey, int fallback)
        {
            var value = GetSetting(environmentKey, configKey, fallback.ToString());
            return int.TryParse(value, out var parsed) ? parsed : fallback;
        }
    }
}