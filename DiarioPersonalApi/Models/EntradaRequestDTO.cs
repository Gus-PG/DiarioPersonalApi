namespace DiarioPersonalApi.Models
{
    public class EntradaRequestDTO
    {
        public int UserId { get; set; }
        public DateTime Fecha { get; set; }
        public required string Contenido { get; set; }
    }
}
