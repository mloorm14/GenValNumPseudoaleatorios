using System;
using System.Collections.Generic;
using System.Linq;
using GenValNumAl.Models;

namespace GenValNumAl.Services;

/// <summary>
/// Generador Fibonacci Retardado (Lagged Fibonacci).
/// Xₙ = (Xₙ₋ⱼ + Xₙ₋ₖ) mod m     Uₙ = Xₙ / m
///
/// Las primeras k filas de la tabla muestran las semillas X₁…Xₖ.
/// Las siguientes n filas muestran los valores generados Xₖ₊₁…Xₖ₊ₙ.
///
/// Restricciones:
///   • 0 < j < k
///   • Se requieren exactamente k semillas en [0, m-1]
///   • Las semillas no pueden ser todas cero
///   • Si m es par, las semillas no pueden ser todas pares
/// </summary>
public sealed class LaggedFibonacciService
{
    public List<GeneratedNumber> Generate(long[] seeds, int j, int k, long m, int additionalN)
    {
        Validate(seeds, j, k, m, additionalN);

        // La secuencia completa: primero las k semillas, luego los additionalN valores
        int   total    = k + additionalN;
        var   seq      = new long[total];
        for (int i = 0; i < k; i++) seq[i] = seeds[i];

        for (int i = k; i < total; i++)
            seq[i] = (seq[i - j] + seq[i - k]) % m;

        // Construir tabla: primeras k filas = semillas, siguientes additionalN = generados
        var results = new List<GeneratedNumber>(total);
        for (int i = 0; i < total; i++)
            results.Add(new GeneratedNumber
            {
                N  = i + 1,
                Xn = seq[i],
                Un = (double)seq[i] / m
            });

        return results;
    }

    // ── Validación ───────────────────────────────────────────────────────────

    private static void Validate(long[] seeds, int j, int k, long m, int additionalN)
    {
        if (additionalN <= 0)
            throw new ValidationException(
                "Error: La cantidad de números adicionales 'n' debe ser un entero positivo mayor que 0.");

        if (m <= 0)
            throw new ValidationException(
                "Error: El módulo 'm' debe ser un entero positivo mayor que 0.");

        if (j <= 0)
            throw new ValidationException(
                "Error: El rezago 'j' debe ser un entero positivo mayor que 0.");

        if (k <= 0)
            throw new ValidationException(
                "Error: El rezago 'k' debe ser un entero positivo mayor que 0.");

        if (j >= k)
            throw new ValidationException(
                $"Error: Los rezagos deben satisfacer 0 < j < k. " +
                $"Actualmente j = {j} ≥ k = {k}. " +
                $"'k' debe ser estrictamente mayor que 'j' para que la recurrencia sea válida.");

        if (seeds == null || seeds.Length == 0)
            throw new ValidationException(
                $"Error: Debe ingresar exactamente k = {k} semillas separadas por comas.");

        if (seeds.Length != k)
            throw new ValidationException(
                $"Error: Se requieren exactamente k = {k} semillas separadas por comas, " +
                $"pero se proporcionaron {seeds.Length} valor(es). " +
                $"El número de semillas debe coincidir con el rezago k.");

        if (seeds.Any(s => s < 0))
            throw new ValidationException(
                "Error: Todas las semillas deben ser enteros no negativos (≥ 0).");

        if (seeds.Any(s => s >= m))
        {
            long bad = seeds.First(s => s >= m);
            throw new ValidationException(
                $"Error: Todas las semillas deben ser menores que m = {m}. " +
                $"Las semillas representan estados iniciales del generador, " +
                $"por lo que deben pertenecer al intervalo [0, m-1]. " +
                $"La semilla {bad} viola esta condición.");
        }

        if (seeds.All(s => s == 0))
            throw new ValidationException(
                "Error: Las semillas iniciales no pueden ser todas cero. " +
                "Si todas las semillas son 0, la recurrencia produce solo ceros: " +
                "(0 + 0) mod m = 0 para siempre.");

        if (m % 2 == 0 && seeds.All(s => s % 2 == 0))
            throw new ValidationException(
                $"Error: Cuando m = {m} es par, las semillas no pueden ser todas pares. " +
                "La suma de dos pares es par, y par mod (número par) sigue siendo par, " +
                "por lo que la secuencia entera quedaría confinada a números pares, " +
                "reduciendo a la mitad el espacio de valores posibles.");
    }
}
