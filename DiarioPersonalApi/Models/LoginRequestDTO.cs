namespace DiarioPersonalApi.Models
{
    public class LoginRequestDTO
    {
        public required string NombreUsuario { get; set; }
        public required string Contraseña { get; set; }
    }
}
