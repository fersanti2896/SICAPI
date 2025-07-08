namespace SICAPI.Shared.Helpers;

public static class TextHelper
{
    /// <summary>
    /// Convierte la primera letra en mayúscula y el resto en minúsculas.
    /// </summary>
    public static string Capitalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        input = input.Trim().ToLower();
        return char.ToUpper(input[0]) + input.Substring(1);
    }

    /// <summary>
    /// Convierte la primera letra de cada palabra en mayúscula.
    /// Ejemplo: "esteban prieto" => "Esteban Prieto"
    /// </summary>
    public static string CapitalizeEachWord(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        return string.Join(' ', input.Trim().ToLower().Split(' ')
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => char.ToUpper(w[0]) + w.Substring(1)));
    }
}
