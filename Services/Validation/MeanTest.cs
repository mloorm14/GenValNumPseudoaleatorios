using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GenValNumAl.Models;

namespace GenValNumAl.Services.Validation;

/// <summary>Prueba de Medias: verifica que la media muestral sea estadísticamente igual a 0.5.</summary>
public sealed class MeanTest : IValidationTest
{
    public string Nombre => "Prueba de Medias";

    public ValidationTestResult Ejecutar(List<double> datos, ValidationParameters parametros)
    {
        int n = datos.Count;
        if (n < 2)
            throw new ValidationException("Se necesitan al menos 2 datos para la prueba de medias.");

        double alpha = parametros.Alpha;
        double z = Statistics.NormalInverseCdf(1 - alpha / 2.0);

        double media = datos.Average();
        double errorEstandar = Math.Sqrt(1.0 / (12.0 * n));
        double li = 0.5 - z * errorEstandar;
        double ls = 0.5 + z * errorEstandar;

        bool aceptaH0 = media >= li && media <= ls;

        var sb = new StringBuilder();
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine("  PRUEBA DE MEDIAS");
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("Hipótesis:");
        sb.AppendLine("  H0: μ = 0.5  (la media de los números generados es 0.5)");
        sb.AppendLine("  H1: μ ≠ 0.5");
        sb.AppendLine();
        sb.AppendLine("Fórmulas:");
        sb.AppendLine("  X̄ = (Σ rᵢ) / n");
        sb.AppendLine("  LI = 0.5 - Z·√(1 / 12n)        LS = 0.5 + Z·√(1 / 12n)");
        sb.AppendLine();
        sb.AppendLine("Datos de entrada:");
        sb.AppendLine($"  n (tamaño de muestra)   = {n}");
        sb.AppendLine($"  α (alpha)               = {alpha.ToString("0.####", CultureInfo.InvariantCulture)}");
        sb.AppendLine($"  Z (crítico, dos colas)  = {z:F4}");
        sb.AppendLine();
        sb.AppendLine("Resultados:");
        sb.AppendLine($"  X̄ (media muestral)     = {media:F6}");
        sb.AppendLine($"  LI (límite inferior)    = {li:F6}");
        sb.AppendLine($"  LS (límite superior)    = {ls:F6}");
        sb.AppendLine();
        sb.AppendLine("Decisión:");
        sb.AppendLine($"  ¿LI ≤ X̄ ≤ LS?   {li:F6} ≤ {media:F6} ≤ {ls:F6}   →   {(aceptaH0 ? "CUMPLE" : "NO CUMPLE")}");
        sb.AppendLine();
        sb.AppendLine($"  >>> {Statistics.Veredicto(aceptaH0)} <<<");

        return new ValidationTestResult { Reporte = sb.ToString(), SeAceptaH0 = aceptaH0 };
    }
}
