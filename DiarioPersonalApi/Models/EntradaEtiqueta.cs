namespace DiarioPersonalApi.Models
{
    public class EntradaEtiqueta
    {
        public int EntradaId { get; set; }
        public int EtiquetaId { get; set; }
        public Entrada Entrada { get; set; }
        public Etiqueta Etiqueta { get; set; }
        public int PosicionInicio { get; set; } // Posición del hashtag en Contenido

    }
}
