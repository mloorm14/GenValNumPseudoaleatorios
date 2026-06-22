using System;
using System.Collections.Generic;
using System.Linq;
using GenValNumAl.Models;

namespace GenValNumAl.Services;

/// <summary>
/// Generador Congruencial Mixto (Hull &amp; Dobell, 1962).
/// Xₙ₊₁ = (a·Xₙ + c) mod m     Uₙ = Xₙ / m
///
/// Para período máximo (= m) se exige el Teorema de Hull-Dobell:
///   1. mcd(c, m) = 1
///   2. (a-1) es divisible por todos los factores primos de m
///   3. Si 4 | m  entonces  4 | (a-1)
/// </summary>
public sealed class MixedCongruentialService
{
    public List<GeneratedNumber> Generate(long x0, long a, long c, long m, int n)
    {
        Validate(x0, a, c, m, n);

        var  results = new List<GeneratedNumber>(n);
        long current = x0;

        for (int i = 1; i <= n; i++)
        {
            current = (a * current + c) % m;
            results.Add(new GeneratedNumber { N = i, Xn = current, Un = (double)current / m });
        }

        return results;
    }

    // ── Validación ───────────────────────────────────────────────────────────

    private static void Validate(long x0, long a, long c, long m, int n)
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

        if (c < 0)
            throw new ValidationException(
                "Error: El incremento 'c' debe ser un entero no negativo (c ≥ 0).");

        if (x0 < 0 || x0 >= m)
            throw new ValidationException(
                $"Error: La semilla X₀ debe estar en el rango [0, m-1] = [0, {m - 1}]. " +
                $"El valor {x0} está fuera de este intervalo.");

        if (n > m)
            throw new ValidationException(
                $"Error: El valor de 'm' define el período máximo. No puede generar {n} " +
                $"números sin repetir si su módulo es apenas {m}. " +
                $"El período máximo del generador congruencial mixto es igual a m = {m}.");

        // ── Teorema de Hull-Dobell ────────────────────────────────────────────

        long gcdCM = Gcd(c, m);
        if (gcdCM != 1)
            throw new ValidationException(
                $"Error (Teorema de Hull-Dobell — condición 1): " +
                $"'c' y 'm' deben ser primos relativos (coprimos), pero mcd({c}, {m}) = {gcdCM} ≠ 1. " +
                $"Comparten el factor común {gcdCM}, lo que reduce el período del generador. " +
                $"Ajuste 'c' para que no comparta divisores con 'm'.");

        var primeFactors  = GetDistinctPrimeFactors(m);
        var badFactors    = primeFactors.Where(p => (a - 1) % p != 0).ToList();
        if (badFactors.Count > 0)
        {
            string fs = string.Join(", ", badFactors);
            throw new ValidationException(
                $"Error (Teorema de Hull-Dobell — condición 2): " +
                $"(a-1) = {a - 1} debe ser divisible por cada factor primo distinto de m = {m}. " +
                $"Los factores [{fs}] no dividen a (a-1). " +
                $"Esta condición asegura que el generador recorra todos los residuos de 0 a m-1.");
        }

        if (m % 4 == 0 && (a - 1) % 4 != 0)
            throw new ValidationException(
                $"Error (Teorema de Hull-Dobell — condición 3): " +
                $"Como m = {m} es múltiplo de 4, también se requiere que (a-1) sea múltiplo de 4. " +
                $"Actualmente (a-1) = {a - 1} y (a-1) mod 4 = {(a - 1) % 4} ≠ 0.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static long Gcd(long a, long b)
    {
        while (b != 0) (a, b) = (b, a % b);
        return Math.Abs(a);
    }

    private static List<long> GetDistinctPrimeFactors(long n)
    {
        var factors = new List<long>();
        for (long i = 2; i * i <= n; i++)
        {
            if (n % i != 0) continue;
            factors.Add(i);
            while (n % i == 0) n /= i;
        }
        if (n > 1) factors.Add(n);
        return factors;
    }
}
