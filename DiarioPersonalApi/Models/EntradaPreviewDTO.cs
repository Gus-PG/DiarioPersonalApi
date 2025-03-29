namespace DiarioPersonalApi.Models
{
    public class EntradaPreviewDTO
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Preview { get; set; }  // Solo 250 caracteres por ejemplo
        public List<string> Etiquetas { get; set; }
    }
}
