using System;
using System.Collections.Generic;
using System.Globalization;

namespace GenValNumAl.Services.Validation;

/// <summary>
/// Interpreta archivos .txt con números separados por saltos de línea y/o espacios,
/// usando coma como separador decimal (formato local), p. ej.: "0,375000\n0,062500".
/// </summary>
public static class NumberFileParser
{
    private static readonly char[] Separadores = { '\n', '\r', ' ', '\t' };

    public static List<double> Parse(string contenido)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            throw new ValidationException("Error al leer el archivo: el archivo está vacío.");

        var tokens = contenido.Split(Separadores, StringSplitOptions.RemoveEmptyEntries);
        var resultado = new List<double>(tokens.Length);

        foreach (var token in tokens)
        {
            string normalizado = token.Replace(',', '.');

            // TryParse acepta literalmente "NaN"/"Infinity"/"-Infinity"; se descartan explícitamente
            // porque arruinarían en silencio todos los cálculos posteriores sin lanzar una excepción.
            bool esValido = double.TryParse(normalizado, NumberStyles.Float, CultureInfo.InvariantCulture, out double valor)
                            && !double.IsNaN(valor) && !double.IsInfinity(valor);

            if (!esValido)
            {
                throw new ValidationException(
                    $"Error al leer el archivo: asegúrese de que solo contenga números (valor inválido: '{token}').");
            }

            resultado.Add(valor);
        }

        if (resultado.Count == 0)
            throw new ValidationException("Error al leer el archivo: no contiene números válidos.");

        return resultado;
    }
}
