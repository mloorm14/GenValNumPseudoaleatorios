using System;
using System.Collections.Generic;

namespace GenValNumAl.Services.Validation;

/// <summary>
/// Aproximaciones numéricas puras (sin integrales ni dependencias externas) usadas por las
/// pruebas de validación: inversa de la normal estándar (algoritmo de Acklam) y el valor
/// crítico Chi-Cuadrado vía la transformación de Wilson-Hilferty.
/// </summary>
public static class Statistics
{
    /// <summary>Inversa de la función de distribución normal estándar (cuantil), algoritmo racional de Acklam.</summary>
    public static double NormalInverseCdf(double p)
    {
        if (p <= 0.0) return double.NegativeInfinity;
        if (p >= 1.0) return double.PositiveInfinity;

        double[] a = {
            -3.969683028665376e+01,  2.209460984245205e+02,
            -2.759285104469687e+02,  1.383577518672690e+02,
            -3.066479806614716e+01,  2.506628277459239e+00
        };
        double[] b = {
            -5.447609879822406e+01,  1.615858368580409e+02,
            -1.556989798598866e+02,  6.680131188771972e+01,
            -1.328068155288572e+01
        };
        double[] c = {
            -7.784894002430293e-03, -3.223964580411365e-01,
            -2.400758277161838e+00, -2.549732539343734e+00,
             4.374664141464968e+00,  2.938163982698783e+00
        };
        double[] d = {
             7.784695709041462e-03,  3.224671290700398e-01,
             2.445134137142996e+00,  3.754408661907416e+00
        };

        const double plow = 0.02425;
        const double phigh = 1 - plow;

        if (p < plow)
        {
            double q = Math.Sqrt(-2 * Math.Log(p));
            return (((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                   ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
        }

        if (p <= phigh)
        {
            double q = p - 0.5;
            double r = q * q;
            return (((((a[0] * r + a[1]) * r + a[2]) * r + a[3]) * r + a[4]) * r + a[5]) * q /
                   (((((b[0] * r + b[1]) * r + b[2]) * r + b[3]) * r + b[4]) * r + 1);
        }

        double qHigh = Math.Sqrt(-2 * Math.Log(1 - p));
        return -(((((c[0] * qHigh + c[1]) * qHigh + c[2]) * qHigh + c[3]) * qHigh + c[4]) * qHigh + c[5]) /
                ((((d[0] * qHigh + d[1]) * qHigh + d[2]) * qHigh + d[3]) * qHigh + 1);
    }

    /// <summary>Valor crítico de la distribución Chi-Cuadrado para "df" grados de libertad en el percentil "p", vía Wilson-Hilferty.</summary>
    public static double ChiSquareCriticalValue(int df, double p)
    {
        if (df <= 0)
            throw new ValidationException("No se puede calcular el valor crítico Chi-Cuadrado: los grados de libertad deben ser mayores a 0 (verifique el tamaño de la muestra y los intervalos).");

        double z = NormalInverseCdf(p);
        double termino = 1.0 - 2.0 / (9.0 * df) + z * Math.Sqrt(2.0 / (9.0 * df));
        double valor = df * Math.Pow(termino, 3);
        return valor < 0 ? 0 : valor;
    }

    /// <summary>Cuenta el número de corridas (rachas) en una secuencia de símbolos discretos (p. ej. 0/1).</summary>
    public static int ContarCorridas(IReadOnlyList<int> secuencia)
    {
        if (secuencia.Count == 0) return 0;

        int corridas = 1;
        for (int i = 1; i < secuencia.Count; i++)
        {
            if (secuencia[i] != secuencia[i - 1])
                corridas++;
        }

        return corridas;
    }

    public static string Veredicto(bool seAceptaH0) => seAceptaH0 ? "SE ACEPTA H0" : "SE RECHAZA H0";
}
