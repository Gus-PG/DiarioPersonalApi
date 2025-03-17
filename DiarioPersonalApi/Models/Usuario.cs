namespace DiarioPersonalApi.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; }
        public string ContraseñaHash { get; set; } // Para JWT más adelante

        //Para confirmar Mail:
        public string Email { get; set; }                   // Mail usuario.
        public bool EmailConfirmed { get; set; }            // Indica si correo está confirmado.
        public string ConfirmationToken { get; set; }       // Token para confirmar
        public DateTime? ConfirmationTokenExpiry { get; set; }

        // Rol del usuario 
        public string Role { get; set; }

        // Propiedades de Navegación:
        public List<Entrada> Entradas { get; set; } // Relación 1:N con Entrada
    }
}
