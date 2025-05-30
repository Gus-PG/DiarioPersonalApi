﻿namespace DiarioPersonalApi.Models
{
    public class Entrada
    {
        public int Id {  get; set; }
        public int UsuarioId {  get; set; }    // Clave foránea
        public required DateTime Fecha { get; set; }
        public required string Contenido { get; set; }

        public Usuario? Usuario { get; set; }    // Propiedad de Navegación.
        public List<EntradaEtiqueta>? EntradasEtiquetas { get; set; }
    }
}
