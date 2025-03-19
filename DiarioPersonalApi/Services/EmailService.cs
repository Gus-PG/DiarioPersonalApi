using System.Net.Mail;
using System.Net;
using DiarioPersonalApi.Settings;

namespace DiarioPersonalApi.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(SmtpSettings smtpSettings)
        {
            _smtpSettings = smtpSettings;
        }

        public async Task EnviarCorreoConfirmacion(string emailDestino, string confirmLink)
        {
            using (var mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(_smtpSettings.User, "DAY2DAY");
                mailMessage.To.Add(emailDestino);
                mailMessage.Subject = "Confirma tu correo";
                mailMessage.Body = $"¡Hola!\n\nGracias por registrarte. Para confirmar tu correo, haz clic en el siguiente enlace:\n{confirmLink}\n\nSi no solicitaste esto, ignora el mensaje.";
                mailMessage.IsBodyHtml = false; // o true si prefieres mandar HTML

                // Configura el cliente SMTP
                using (var smtpClient = new SmtpClient(_smtpSettings.Server))
                {
                    smtpClient.Port = _smtpSettings.Port;
                    smtpClient.Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Pass);
                    smtpClient.EnableSsl = true;                    // o false según tu hosting                                                  
                    await smtpClient.SendMailAsync(mailMessage);    // Enviar
                }
            }
        }
    }
}
