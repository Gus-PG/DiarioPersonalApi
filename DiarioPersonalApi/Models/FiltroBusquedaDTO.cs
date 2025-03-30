namespace DiarioPersonalApi.Models
{
    public class FiltroBusquedaDTO
    {
        public string Texto { get; set; }  // Texto libre a buscar
        public List<string> Etiquetas { get; set; } = new();  // Lista de etiquetas seleccionadas
        // La inicializamos vacía para evitar errores de null.
    }
}
