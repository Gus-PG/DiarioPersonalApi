namespace DiarioPersonalApi.Models
{
    public class ChangePasswordRequestDTO
    {
        public string CurrentPasword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
