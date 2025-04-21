namespace DiarioPersonalApi.Settings
{
    public class SmtpSettings
    {
        public string Server { get; set; }      // 🟢 Nombre del host SMTP
        public int Port { get; set; }           // 🟢 Puerto (ej. 587 o 465)
        public bool EnableSsl { get; set; }     // 🟢 Campo para control de SSL
        public string User { get; set; }        // 🟢 Usuario de la cuenta
        public string Pass { get; set; }        // 🟢 Contraseña SMTP
    }
}
