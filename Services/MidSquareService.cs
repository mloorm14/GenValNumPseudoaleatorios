using System;
using System.Collections.Generic;
using GenValNumAl.Models;

namespace GenValNumAl.Services;

/// <summary>
/// Generador de Mitad del Cuadrado (von Neumann, 1946).
/// Xₙ₊₁ = dígitos centrales de Xₙ²     Uₙ = Xₙ / 10^d
/// </summary>
public sealed class MidSquareService
{
    public List<GeneratedNumber> Generate(long x0, int digits, int n)
    {
        Validate(x0, digits, n);

        var results = new List<GeneratedNumber>(n);
        long current = x0;
        long modulus  = (long)Math.Pow(10, digits);

        for (int i = 1; i <= n; i++)
        {
            double un      = (double)current / modulus;
            results.Add(new GeneratedNumber { N = i, Xn = current, Un = un });
            current = ExtractMiddleDigits(current * current, digits);
        }

        return results;
    }

    // ── Validación ───────────────────────────────────────────────────────────

    private static void Validate(long x0, int digits, int n)
    {
        if (n <= 0)
            throw new ValidationException(
                "Error: La cantidad de números 'n' debe ser un entero positivo mayor que 0.");

        if (digits < 2 || digits % 2 != 0 || digits > 8)
            throw new ValidationException(
                "Error: El número de dígitos 'd' debe ser un entero par entre 2 y 8 " +
                "(ej: 2, 4, 6 u 8). Valores impares o mayores de 8 causan desbordamiento " +
                "aritmético o no permiten extraer la mitad correctamente.");

        if (x0 <= 0)
            throw new ValidationException(
                "Error: La semilla X₀ debe ser un entero positivo mayor que 0.");

        long maxSeed = (long)Math.Pow(10, digits) - 1;
        if (x0 > maxSeed)
            throw new ValidationException(
                $"Error: Para d = {digits} dígitos la semilla debe estar en [1, {maxSeed}]. " +
                $"El valor X₀ = {x0} tiene más de {digits} dígitos y no es válido para este parámetro.");
    }

    // ── Algoritmo ────────────────────────────────────────────────────────────

    private static long ExtractMiddleDigits(long squared, int digits)
    {
        // Rellenar con ceros a la izquierda hasta tener 2·d dígitos
        string s     = squared.ToString().PadLeft(digits * 2, '0');
        int    start = digits / 2;
        return long.Parse(s.Substring(start, digits));
    }
}
