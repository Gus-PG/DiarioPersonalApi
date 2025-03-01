namespace DiarioPersonalApi.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public required string NombreUsuario { get; set; }
        public required string ContraseñaHash { get; set; } // Para JWT más adelante
        public List<Entrada> Entradas { get; set; } // Relación 1:N con Entrada

    }
}
