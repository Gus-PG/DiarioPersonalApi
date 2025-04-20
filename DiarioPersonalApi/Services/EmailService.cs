using System.Net.Mail;
using System.Net;
using DiarioPersonalApi.Settings;
using Microsoft.Extensions.Options;

namespace DiarioPersonalApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task EnviarCorreoConfirmacion(string emailDestino, string confirmLink)
        {
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.User, "DAY2DAY"),
                Subject = "Confirma tu correo",
                Body = $"¡Hola!\n\nGracias por registrarte. Para confirmar tu correo, haz clic en el siguiente enlace:\n{confirmLink}\n\nSi no solicitaste esto, ignora el mensaje.",
                IsBodyHtml = false
            };

            mailMessage.To.Add(emailDestino);

            using var smtpClient = new SmtpClient(_smtpSettings.Server)
            {
                Port = _smtpSettings.Port,
                Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Pass),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }


        public async Task EnviarCorreoRecuperacion(string emailDestino, string resetLink)
        {
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.User, "DAY2DAY"),
                Subject = "Recuperar contraseña",
                Body = $"Hemos recibido una solicitud para cambiar tu contraseña. Si fuiste tú, haz clic en el siguiente enlace:\n\n{resetLink}\n\nEste enlace es válido por 24 horas.",
                IsBodyHtml = false
            };

            mailMessage.To.Add(emailDestino);

            using var smtpClient = new SmtpClient(_smtpSettings.Server)
            {
                Port = _smtpSettings.Port,
                Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Pass),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
