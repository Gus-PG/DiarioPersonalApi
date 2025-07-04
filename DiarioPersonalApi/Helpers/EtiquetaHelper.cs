using System.Text.RegularExpressions;

namespace DiarioPersonalApi.Helpers;

public static class EtiquetaHelper
{
    public static List<String> ExtraerEtiquetasDesdeTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return new List<String>();

        var etiquetas = Regex.Matches(texto, @"#(\w+)")
            .Select(m => m.Groups[1].Value.ToUpperInvariant())
            .Distinct()
            .ToList();

        return etiquetas;
    } 
}
