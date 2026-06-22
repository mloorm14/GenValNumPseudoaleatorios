using System;
using System.Collections.Generic;
using GenValNumAl.Models;

namespace GenValNumAl.Services;

/// <summary>
/// Generador Congruencial Multiplicativo.
/// Xₙ₊₁ = (a·Xₙ) mod m     Uₙ = Xₙ / m
///
/// Restricciones críticas:
///   • X₀ ≠ 0  (semilla cero produce secuencia de puros ceros)
///   • mcd(X₀, m) = 1
/// </summary>
public sealed class MultiplicativeCongruentialService
{
    public List<GeneratedNumber> Generate(long x0, long a, long m, int n)
    {
        Validate(x0, a, m, n);

        var  results = new List<GeneratedNumber>(n);
        long current = x0;

        for (int i = 1; i <= n; i++)
        {
            current = (a * current) % m;
            results.Add(new GeneratedNumber { N = i, Xn = current, Un = (double)current / m });
        }

        return results;
    }

    // ── Validación ───────────────────────────────────────────────────────────

    private static void Validate(long x0, long a, long m, int n)
    {
        if (n <= 0)
            throw new ValidationException(
                "Error: La cantidad de números 'n' debe ser un entero positivo mayor que 0.");

        if (m <= 0)
            throw new ValidationException(
                "Error: El módulo 'm' debe ser un entero positivo mayor que 0.");

        if (a <= 0)
            throw new ValidationException(
                "Error: El multiplicador 'a' debe ser un entero positivo mayor que 0.");

        if (x0 == 0)
            throw new ValidationException(
                "Error: La semilla X₀ no puede ser 0 en el Generador Congruencial Multiplicativo. " +
                "Si X₀ = 0 entonces X₁ = (a × 0) mod m = 0 y toda la secuencia será cero, " +
                "lo que no genera ningún número pseudoaleatorio útil.");

        if (x0 < 0 || x0 >= m)
            throw new ValidationException(
                $"Error: La semilla X₀ debe estar en el rango [1, m-1] = [1, {m - 1}]. " +
                $"El valor {x0} está fuera de este intervalo.");

        long gcd = Gcd(x0, m);
        if (gcd != 1)
            throw new ValidationException(
                $"Error: X₀ y 'm' deben ser coprimos (mcd = 1) para garantizar un período largo. " +
                $"mcd({x0}, {m}) = {gcd} ≠ 1. Comparten el factor {gcd}, lo que " +
                $"acorta drásticamente el período y degrada la calidad de la secuencia.");

        if (n >= m)
            throw new ValidationException(
                $"Error: El valor de 'm' define el período máximo. No puede generar {n} " +
                $"números sin repetir si su módulo es apenas {m}. " +
                $"El período máximo del generador multiplicativo es m-1 = {m - 1}.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static long Gcd(long a, long b)
    {
        while (b != 0) (a, b) = (b, a % b);
        return Math.Abs(a);
    }
}
