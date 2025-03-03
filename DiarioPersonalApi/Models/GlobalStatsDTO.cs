namespace DiarioPersonalApi.Models
{
    public class GlobalStatsDTO
    {
        public int TotalEntradas { get; set; }
        public int TotalUsuarios { get; set; }
        public string? UsuarioMasActivo { get; set; }
        public int EntradasUsuarioMasActivo { get; set; }
        public string? HashtagMasUsado { get; set; }
        public int UsosHashtagMasUsado { get; set; }
        public DateTime? DiaMasActivo { get; set; }
        public int EntradasDiaMasActivo { get; set; }
    }
}
