using System.Net.Mail;
using System.Net;

namespace DiarioPersonalApi.Services
{
    public class EmailService
    {
        public async Task EnviarCorreoConfirmacion(string emailDestino, string confirmLink)
        {
            // Configuración de tu SMTP
            string smtpServer = "mail.tuhosting.com";  // El servidor SMTP (consulta tu proveedor)
            int smtpPort = 587;                        // O 25, 465, depende de tu hosting
            string smtpUser = "tuusuario@tudominio.com";
            string smtpPass = "TuPasswordSMTP";

            using (var mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress("noreply@tudominio.com", "Diario Personal");
                mailMessage.To.Add(emailDestino);
                mailMessage.Subject = "Confirma tu correo";
                mailMessage.Body = $"¡Hola!\n\nGracias por registrarte. Para confirmar tu correo, haz clic en el siguiente enlace:\n{confirmLink}\n\nSi no solicitaste esto, ignora el mensaje.";
                mailMessage.IsBodyHtml = false; // o true si prefieres mandar HTML

                // Configura el cliente SMTP
                using (var smtpClient = new SmtpClient(smtpServer))
                {
                    smtpClient.Port = smtpPort;
                    smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    smtpClient.EnableSsl = true;  // o false según tu hosting
                                                  // Enviar
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
        }
    }
}
