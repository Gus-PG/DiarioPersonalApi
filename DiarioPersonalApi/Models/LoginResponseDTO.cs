namespace DiarioPersonalApi.Models
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
