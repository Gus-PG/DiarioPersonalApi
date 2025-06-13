namespace DiarioPersonalApi.Models
{
    public class FiltroBusquedaDTO
    {
        public string Texto { get; set; }  // Texto libre a buscar
        public List<string> Etiquetas { get; set; } = new();  // La inicializamos vacía para evitar errores de null.        
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool EsBusquedaAnd { get; set; } = false; // false = OR, true = AND
    }
}
