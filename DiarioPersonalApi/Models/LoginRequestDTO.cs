namespace DiarioPersonalApi.Models
{
    public class LoginRequestDTO
    {
        public required string Email { get; set; }
        public required string Contraseña { get; set; }
    }
}
