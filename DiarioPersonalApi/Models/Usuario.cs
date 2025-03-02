namespace DiarioPersonalApi.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public required string NombreUsuario { get; set; }
        // ************ OJO: **************
        // Por ahora lo dejamos sin ser requerido. Habrá que hacerlo antes de acabar la app
        public string ContraseñaHash { get; set; } // Para JWT más adelante
        public List<Entrada> Entradas { get; set; } // Relación 1:N con Entrada

    }
}
