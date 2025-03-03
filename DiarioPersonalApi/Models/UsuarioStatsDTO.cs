namespace DiarioPersonalApi.Models
{
    public class UsuarioStatsDTO
    {
        public int TotalEntradas { get; set; }
        public int TotalEtiquetas { get; set; }
        public Dictionary<string, int> EtiquetasMasUsadas { get; set; }
        public DateTime? DiaMasActivo { get; set; }
    }
}
