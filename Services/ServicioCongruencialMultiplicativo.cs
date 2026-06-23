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
public sealed class ServicioCongruencialMultiplicativo
{
    public ResultadoGeneracion Generar(long x0, long a, long m, int n)
    {
        Validar(x0, a, m, n);

        var  resultados = new List<NumeroGenerado>(n);
        long actual     = x0;

        InfoCiclo? ciclo = null;
        var vistos = new Dictionary<long, int> { [x0] = 0 }; // X₀ ocupa la "iteración 0"

        for (int i = 1; i <= n; i++)
        {
            actual = (a * actual) % m;
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

    private static void Validar(long x0, long a, long m, int n)
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

        if (x0 == 0)
            throw new ExcepcionValidacion(
                "Error: La semilla X₀ no puede ser 0 en el Generador Congruencial Multiplicativo. " +
                "Si X₀ = 0 entonces X₁ = (a × 0) mod m = 0 y toda la secuencia será cero, " +
                "lo que no genera ningún número pseudoaleatorio útil.");

        if (x0 < 0 || x0 >= m)
            throw new ExcepcionValidacion(
                $"Error: La semilla X₀ debe estar en el rango [1, m-1] = [1, {m - 1}]. " +
                $"El valor {x0} está fuera de este intervalo.");

        long mcd = Mcd(x0, m);
        if (mcd != 1)
            throw new ExcepcionValidacion(
                $"Error: X₀ y 'm' deben ser coprimos (mcd = 1) para garantizar un período largo. " +
                $"mcd({x0}, {m}) = {mcd} ≠ 1. Comparten el factor {mcd}, lo que " +
                $"acorta drásticamente el período y degrada la calidad de la secuencia.");

        if (n >= m)
            throw new ExcepcionValidacion(
                $"Error: El valor de 'm' define el período máximo. No puede generar {n} " +
                $"números sin repetir si su módulo es apenas {m}. " +
                $"El período máximo del generador multiplicativo es m-1 = {m - 1}.");
    }

    // ── Funciones auxiliares ─────────────────────────────────────────────────

    private static long Mcd(long a, long b)
    {
        while (b != 0) (a, b) = (b, a % b);
        return Math.Abs(a);
    }
}
