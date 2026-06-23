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
public sealed class ServicioCongruencialMixto
{
    public ResultadoGeneracion Generar(long x0, long a, long c, long m, int n)
    {
        Validar(x0, a, c, m, n);

        var  resultados = new List<NumeroGenerado>(n);
        long actual     = x0;

        InfoCiclo? ciclo = null;
        var vistos = new Dictionary<long, int> { [x0] = 0 }; // X₀ ocupa la "iteración 0"

        for (int i = 1; i <= n; i++)
        {
            actual = (a * actual + c) % m;
            resultados.Add(new NumeroGenerado { N = i, Xn = actual, Un = (double)actual / m });

            if (ciclo is null)
            {
                if (vistos.TryGetValue(actual, out int primeraIteracion))
                {
                    int periodo = i - primeraIteracion;
                    string origen = primeraIteracion == 0
                        ? "la semilla inicial (iteración 0)"
                        : $"la iteración {primeraIteracion}";
                    ciclo = new InfoCiclo
                    {
                        IteracionInicio    = primeraIteracion,
                        IteracionDeteccion = i,
                        LongitudPeriodo    = periodo,
                        Mensaje = $"¡Ciclo detectado! El periodo máximo antes de repetirse fue de {periodo} iteraciones " +
                                  $"(Xₙ en la iteración {i} repite el valor visto en {origen})."
                    };
                }
                else
                {
                    vistos[actual] = i;
                }
            }
        }

        return new ResultadoGeneracion { Numeros = resultados, Ciclo = ciclo };
    }

    // ── Validación ───────────────────────────────────────────────────────────

    private static void Validar(long x0, long a, long c, long m, int n)
    {
        if (n <= 0)
            throw new ExcepcionValidacion(
                "Error: La cantidad de números 'n' debe ser un entero positivo mayor que 0.");

        if (m <= 0)
            throw new ExcepcionValidacion(
                "Error: El módulo 'm' debe ser un entero positivo mayor que 0.");

        if (a <= 0)
            throw new ExcepcionValidacion(
                "Error: El multiplicador 'a' debe ser un entero positivo mayor que 0.");

        if (c < 0)
            throw new ExcepcionValidacion(
                "Error: El incremento 'c' debe ser un entero no negativo (c ≥ 0).");

        if (x0 < 0 || x0 >= m)
            throw new ExcepcionValidacion(
                $"Error: La semilla X₀ debe estar en el rango [0, m-1] = [0, {m - 1}]. " +
                $"El valor {x0} está fuera de este intervalo.");

        if (n > m)
            throw new ExcepcionValidacion(
                $"Error: El valor de 'm' define el período máximo. No puede generar {n} " +
                $"números sin repetir si su módulo es apenas {m}. " +
                $"El período máximo del generador congruencial mixto es igual a m = {m}.");

        // ── Teorema de Hull-Dobell ────────────────────────────────────────────

        long mcdCM = Mcd(c, m);
        if (mcdCM != 1)
            throw new ExcepcionValidacion(
                $"Error (Teorema de Hull-Dobell — condición 1): " +
                $"'c' y 'm' deben ser primos relativos (coprimos), pero mcd({c}, {m}) = {mcdCM} ≠ 1. " +
                $"Comparten el factor común {mcdCM}, lo que reduce el período del generador. " +
                $"Ajuste 'c' para que no comparta divisores con 'm'.");

        var factoresPrimos  = ObtenerFactoresPrimosDistintos(m);
        var factoresInvalidos = factoresPrimos.Where(p => (a - 1) % p != 0).ToList();
        if (factoresInvalidos.Count > 0)
        {
            string lista = string.Join(", ", factoresInvalidos);
            throw new ExcepcionValidacion(
                $"Error (Teorema de Hull-Dobell — condición 2): " +
                $"(a-1) = {a - 1} debe ser divisible por cada factor primo distinto de m = {m}. " +
                $"Los factores [{lista}] no dividen a (a-1). " +
                $"Esta condición asegura que el generador recorra todos los residuos de 0 a m-1.");
        }

        if (m % 4 == 0 && (a - 1) % 4 != 0)
            throw new ExcepcionValidacion(
                $"Error (Teorema de Hull-Dobell — condición 3): " +
                $"Como m = {m} es múltiplo de 4, también se requiere que (a-1) sea múltiplo de 4. " +
                $"Actualmente (a-1) = {a - 1} y (a-1) mod 4 = {(a - 1) % 4} ≠ 0.");
    }

    // ── Funciones auxiliares ─────────────────────────────────────────────────

    private static long Mcd(long a, long b)
    {
        while (b != 0) (a, b) = (b, a % b);
        return Math.Abs(a);
    }

    private static List<long> ObtenerFactoresPrimosDistintos(long n)
    {
        var factores = new List<long>();
        for (long i = 2; i * i <= n; i++)
        {
            if (n % i != 0) continue;
            factores.Add(i);
            while (n % i == 0) n /= i;
        }
        if (n > 1) factores.Add(n);
        return factores;
    }
}
