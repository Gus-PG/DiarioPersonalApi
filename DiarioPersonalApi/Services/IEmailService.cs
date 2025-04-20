namespace DiarioPersonalApi.Services
{
    public interface IEmailService
    {
        Task EnviarCorreoConfirmacion(string emailDestino, string confirmLink);
        Task EnviarCorreoRecuperacion(string emailDestino, string resetLink);
    }
}
