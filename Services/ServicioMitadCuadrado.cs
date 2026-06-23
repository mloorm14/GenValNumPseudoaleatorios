using System;
using System.Collections.Generic;
using System.Globalization;
using GenValNumAl.Models;

namespace GenValNumAl.Services;

/// <summary>
/// Generador de Mitad del Cuadrado (von Neumann, 1946).
/// Xₙ₊₁ = dígitos centrales de Xₙ²     Uₙ = Xₙ / 10^d
/// </summary>
public sealed class ServicioMitadCuadrado
{
    public ResultadoGeneracion Generar(long x0, int digitos, int n)
    {
        Validar(x0, digitos, n);

        var resultados = new List<NumeroGenerado>(n);
        long actual     = x0;
        long modulo     = (long)Math.Pow(10, digitos);

        InfoCiclo? ciclo = null;
        var vistos = new Dictionary<long, int>(); // valor de Xₙ → primera iteración en que apareció

        for (int i = 1; i <= n; i++)
        {
            double un = (double)actual / modulo;
            resultados.Add(new NumeroGenerado { N = i, Xn = actual, Un = un });

            if (ciclo is null)
            {
                if (actual == 0)
                {
                    ciclo = new InfoCiclo
                    {
                        IteracionInicio    = i,
                        IteracionDeteccion = i,
                        LongitudPeriodo    = 1,
                        Mensaje = $"La secuencia degeneró en la iteración {i}: Xₙ llegó a 0 y se mantendrá en 0 indefinidamente."
                    };
                }
                else if (vistos.TryGetValue(actual, out int primeraIteracion))
                {
                    int periodo = i - primeraIteracion;
                    ciclo = new InfoCiclo
                    {
                        IteracionInicio    = primeraIteracion,
                        IteracionDeteccion = i,
                        LongitudPeriodo    = periodo,
                        Mensaje = $"¡Ciclo detectado! El periodo máximo antes de repetirse fue de {periodo} iteraciones " +
                                  $"(Xₙ en la iteración {i} repite el valor visto en la iteración {primeraIteracion})."
                    };
                }
                else
                {
                    vistos[actual] = i;
                }
            }

            actual = ExtraerDigitosCentrales(actual * actual, digitos);
        }

        return new ResultadoGeneracion { Numeros = resultados, Ciclo = ciclo };
    }

    // ── Validación ───────────────────────────────────────────────────────────

    private static void Validar(long x0, int digitos, int n)
    {
        if (n <= 0)
            throw new ExcepcionValidacion(
                "Error: La cantidad de números 'n' debe ser un entero positivo mayor que 0.");

        if (digitos < 2 || digitos % 2 != 0 || digitos > 8)
            throw new ExcepcionValidacion(
                "Error: El número de dígitos 'd' debe ser un entero par entre 2 y 8 " +
                "(ej: 2, 4, 6 u 8). Valores impares o mayores de 8 causan desbordamiento " +
                "aritmético o no permiten extraer la mitad correctamente.");

        if (x0 <= 0)
            throw new ExcepcionValidacion(
                "Error: La semilla X₀ debe ser un entero positivo mayor que 0.");

        int digitosSemilla = x0.ToString(CultureInfo.InvariantCulture).Length;
        if (digitosSemilla != digitos)
            throw new ExcepcionValidacion(
                "La semilla X0 debe tener exactamente la misma cantidad de dígitos especificada en 'd'. " +
                $"Actualmente X0 = {x0} tiene {digitosSemilla} dígito(s) y d = {digitos}.");
    }

    // ── Algoritmo ────────────────────────────────────────────────────────────

    private static long ExtraerDigitosCentrales(long cuadrado, int digitos)
    {
        // Rellenar con ceros a la izquierda hasta tener 2·d dígitos
        string texto  = cuadrado.ToString().PadLeft(digitos * 2, '0');
        int    inicio = digitos / 2;
        return long.Parse(texto.Substring(inicio, digitos));
    }
}
