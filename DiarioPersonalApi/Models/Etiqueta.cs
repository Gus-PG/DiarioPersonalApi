namespace DiarioPersonalApi.Models
{
    public class Etiqueta
    {
        public int Id { get; set; }
        public required string Nombre { get; set; } // Ej: "Toby"
        public List<EntradaEtiqueta> EntradasEtiquetas { get; set; } // Relación N:N

    }
}
