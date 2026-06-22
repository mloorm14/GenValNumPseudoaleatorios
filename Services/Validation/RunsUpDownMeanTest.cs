using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GenValNumAl.Models;

namespace GenValNumAl.Services.Validation;

/// <summary>Corridas Arriba y Abajo de la Media: analiza rachas de valores por encima/debajo de 0.5.</summary>
public sealed class RunsUpDownMeanTest : IValidationTest
{
    public string Nombre => "Corridas Arriba y Abajo de la Media";

    public ValidationTestResult Ejecutar(List<double> datos, ValidationParameters parametros)
    {
        int n = datos.Count;
        if (n < 2)
            throw new ValidationException("Se necesitan al menos 2 datos para la prueba de corridas arriba y abajo de la media.");

        double alpha = parametros.Alpha;

        var secuencia = datos.Select(x => x > 0.5 ? 1 : 0).ToList();
        int n1 = secuencia.Count(s => s == 1);
        int n0 = n - n1;

        if (n0 == 0 || n1 == 0)
            throw new ValidationException("Todos los valores quedaron del mismo lado de la media (0.5); no se puede aplicar la prueba.");

        int corridas = Statistics.ContarCorridas(secuencia);

        double mu = (2.0 * n0 * n1) / n + 0.5;
        double varianza = (2.0 * n0 * n1 * (2.0 * n0 * n1 - n)) / (Math.Pow(n, 2) * (n - 1));

        if (varianza <= 0)
            throw new ValidationException("La varianza calculada no es válida con estos datos (desbalance extremo entre valores mayores y menores a 0.5).");

        double z0 = (corridas - mu) / Math.Sqrt(varianza);

        double zCritico = Statistics.NormalInverseCdf(1 - alpha / 2.0);
        bool aceptaH0 = Math.Abs(z0) <= zCritico;

        var sb = new StringBuilder();
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine("  CORRIDAS ARRIBA Y ABAJO DE LA MEDIA");
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("Hipótesis:");
        sb.AppendLine("  H0: la secuencia de valores por encima/debajo de 0.5 es independiente (aleatoria)");
        sb.AppendLine("  H1: la secuencia NO es independiente");
        sb.AppendLine();
        sb.AppendLine("Fórmulas:");
        sb.AppendLine("  μ = (2·n0·n1) / n + 0.5");
        sb.AppendLine("  σ² = [2·n0·n1·(2·n0·n1 - n)] / [n²·(n-1)]");
        sb.AppendLine("  Z₀ = (C₀ - μ) / σ              Crítico: Z(1-α/2)");
        sb.AppendLine();
        sb.AppendLine("Datos de entrada:");
        sb.AppendLine($"  n (tamaño de muestra) = {n}");
        sb.AppendLine($"  n0 (valores ≤ 0.5)    = {n0}");
        sb.AppendLine($"  n1 (valores > 0.5)    = {n1}");
        sb.AppendLine($"  α (alpha)             = {alpha.ToString("0.####", CultureInfo.InvariantCulture)}");
        sb.AppendLine();
        sb.AppendLine("Resultados:");
        sb.AppendLine($"  C₀ (corridas observadas) = {corridas}");
        sb.AppendLine($"  μ (esperado)             = {mu:F6}");
        sb.AppendLine($"  σ² (varianza)            = {varianza:F6}");
        sb.AppendLine($"  Z₀ (estadístico)         = {z0:F6}");
        sb.AppendLine($"  Z crítico                = {zCritico:F4}");
        sb.AppendLine();
        sb.AppendLine("Decisión:");
        sb.AppendLine($"  ¿|Z₀| ≤ Z crítico?   {Math.Abs(z0):F6} ≤ {zCritico:F4}   →   {(aceptaH0 ? "CUMPLE" : "NO CUMPLE")}");
        sb.AppendLine();
        sb.AppendLine($"  >>> {Statistics.Veredicto(aceptaH0)} <<<");

        return new ValidationTestResult { Reporte = sb.ToString(), SeAceptaH0 = aceptaH0 };
    }
}
