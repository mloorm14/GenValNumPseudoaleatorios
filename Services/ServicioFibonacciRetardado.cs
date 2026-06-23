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
///   • 0 &lt; j &lt; k
///   • Se requieren exactamente k semillas en [0, m-1]
///   • Las semillas no pueden ser todas cero
///   • Si m es par, las semillas no pueden ser todas pares
/// </summary>
public sealed class ServicioFibonacciRetardado
{
    public ResultadoGeneracion Generar(long[] semillas, int j, int k, long m, int nAdicional)
    {
        Validar(semillas, j, k, m, nAdicional);

        // Secuencia completa: primero las k semillas, luego los nAdicional valores generados
        int total       = k + nAdicional;
        var secuencia   = new long[total];
        for (int i = 0; i < k; i++) secuencia[i] = semillas[i];

        InfoCiclo? ciclo = null;
        var vistos = new Dictionary<string, int>(); // ventana de los últimos k estados → fila (1-based) en que apareció

        for (int i = k; i < total; i++)
        {
            secuencia[i] = (secuencia[i - j] + secuencia[i - k]) % m;

            // La ventana de los k estados más recientes determina por completo la evolución futura:
            // si esa ventana ya se observó antes, la secuencia repetirá exactamente el mismo patrón.
            if (ciclo is null)
            {
                string ventana = string.Join(",", secuencia.Skip(i - k + 1).Take(k));
                int filaActual = i + 1;

                if (vistos.TryGetValue(ventana, out int primeraFila))
                {
                    int periodo = filaActual - primeraFila;
                    ciclo = new InfoCiclo
                    {
                        IteracionInicio    = primeraFila,
                        IteracionDeteccion = filaActual,
                        LongitudPeriodo    = periodo,
                        Mensaje = $"¡Ciclo detectado! El periodo máximo antes de repetirse fue de {periodo} iteraciones " +
                                  $"(el estado de la fila {filaActual} repite el observado en la fila {primeraFila})."
                    };
                }
                else
                {
                    vistos[ventana] = filaActual;
                }
            }
        }

        var resultados = new List<NumeroGenerado>(total);
        for (int i = 0; i < total; i++)
            resultados.Add(new NumeroGenerado
            {
                N  = i + 1,
                Xn = secuencia[i],
                Un = (double)secuencia[i] / m
            });

        return new ResultadoGeneracion { Numeros = resultados, Ciclo = ciclo };
    }

    // ── Validación ───────────────────────────────────────────────────────────

    private static void Validar(long[] semillas, int j, int k, long m, int nAdicional)
    {
        if (nAdicional <= 0)
            throw new ExcepcionValidacion(
                "Error: La cantidad de números adicionales 'n' debe ser un entero positivo mayor que 0.");

        if (m <= 0)
            throw new ExcepcionValidacion(
                "Error: El módulo 'm' debe ser un entero positivo mayor que 0.");

        if (j <= 0)
            throw new ExcepcionValidacion(
                "Error: El rezago 'j' debe ser un entero positivo mayor que 0.");

        if (k <= 0)
            throw new ExcepcionValidacion(
                "Error: El rezago 'k' debe ser un entero positivo mayor que 0.");

        if (j >= k)
            throw new ExcepcionValidacion(
                $"Error: Los rezagos deben satisfacer 0 < j < k. " +
                $"Actualmente j = {j} ≥ k = {k}. " +
                $"'k' debe ser estrictamente mayor que 'j' para que la recurrencia sea válida.");

        if (semillas == null || semillas.Length == 0)
            throw new ExcepcionValidacion(
                $"Error: Debe ingresar exactamente k = {k} semillas separadas por comas.");

        if (semillas.Length != k)
            throw new ExcepcionValidacion(
                "Debe ingresar exactamente 'k' semillas separadas por comas. " +
                $"Actualmente k = {k} pero se proporcionaron {semillas.Length} semilla(s).");

        if (semillas.Any(s => s < 0))
            throw new ExcepcionValidacion(
                "Error: Todas las semillas deben ser enteros no negativos (≥ 0).");

        if (semillas.Any(s => s >= m))
        {
            long invalida = semillas.First(s => s >= m);
            throw new ExcepcionValidacion(
                $"Error: Todas las semillas deben ser menores que m = {m}. " +
                $"Las semillas representan estados iniciales del generador, " +
                $"por lo que deben pertenecer al intervalo [0, m-1]. " +
                $"La semilla {invalida} viola esta condición.");
        }

        if (semillas.All(s => s == 0))
            throw new ExcepcionValidacion(
                "Error: Las semillas iniciales no pueden ser todas cero. " +
                "Si todas las semillas son 0, la recurrencia produce solo ceros: " +
                "(0 + 0) mod m = 0 para siempre.");

        if (m % 2 == 0 && semillas.All(s => s % 2 == 0))
            throw new ExcepcionValidacion(
                $"Error: Cuando m = {m} es par, las semillas no pueden ser todas pares. " +
                "La suma de dos pares es par, y par mod (número par) sigue siendo par, " +
                "por lo que la secuencia entera quedaría confinada a números pares, " +
                "reduciendo a la mitad el espacio de valores posibles.");
    }
}
