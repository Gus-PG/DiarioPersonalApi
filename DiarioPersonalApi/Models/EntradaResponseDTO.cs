namespace DiarioPersonalApi.Models
{
    public class EntradaResponseDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Fecha { get; set; }
        public string Contenido { get; set; }
        public string NombreUsuario { get; set; }
        public List<string> Etiquetas { get; set; }
    }
}
