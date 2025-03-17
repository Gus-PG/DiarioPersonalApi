namespace DiarioPersonalApi.Models
{
    public class RegisterRequestDTO
    {
        public required string NombreUsuario { get; set; }
        public required string Email { get; set; }
        public required string Contraseña { get; set; }
    }
}
