using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GenValNumAl.Models;

namespace GenValNumAl.Services.Validation;

/// <summary>Prueba Kolmogorov-Smirnov: compara la función de distribución empírica con la uniforme(0,1).</summary>
public sealed class PruebaKolmogorovSmirnov : IPruebaValidacion
{
    public string Nombre => "Prueba Kolmogorov-Smirnov";

    public ResultadoPruebaValidacion Ejecutar(List<double> datos, ParametrosValidacion parametros)
    {
        int n = datos.Count;
        if (n < 1)
            throw new ExcepcionValidacion("Se necesita al menos 1 dato para la prueba Kolmogorov-Smirnov.");

        double alpha = parametros.Alpha;
        var ordenados = datos.OrderBy(x => x).ToList();

        double dMasMax = double.MinValue;
        double dMenosMax = double.MinValue;

        for (int i = 1; i <= n; i++)
        {
            double r = ordenados[i - 1];
            double dMas = (double)i / n - r;
            double dMenos = r - (double)(i - 1) / n;
            if (dMas > dMasMax) dMasMax = dMas;
            if (dMenos > dMenosMax) dMenosMax = dMenos;
        }

        double d = Math.Max(dMasMax, dMenosMax);

        // Para α = 0.05 el valor exacto tabulado es 1.36/√n; para otros valores se usa la aproximación asintótica.
        double coeficiente = Math.Abs(alpha - 0.05) < 1e-9
            ? 1.36
            : Math.Sqrt(-0.5 * Math.Log(alpha / 2.0));
        double critico = coeficiente / Math.Sqrt(n);

        bool aceptaH0 = d <= critico;

        var sb = new StringBuilder();
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine("  PRUEBA KOLMOGOROV-SMIRNOV");
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("Hipótesis:");
        sb.AppendLine("  H0: los números provienen de una distribución uniforme(0,1)");
        sb.AppendLine("  H1: los números NO provienen de una distribución uniforme(0,1)");
        sb.AppendLine();
        sb.AppendLine("Fórmulas:");
        sb.AppendLine("  D⁺ = máx( i/n - r₍ᵢ₎ )      D⁻ = máx( r₍ᵢ₎ - (i-1)/n )");
        sb.AppendLine("  D  = máx(D⁺, D⁻)            Crítico = 1.36 / √n   (α = 0.05)");
        sb.AppendLine();
        sb.AppendLine("Datos de entrada:");
        sb.AppendLine($"  n (tamaño de muestra) = {n}");
        sb.AppendLine($"  α (alpha)             = {alpha.ToString("0.####", CultureInfo.InvariantCulture)}");
        sb.AppendLine();
        sb.AppendLine("Resultados:");
        sb.AppendLine($"  D⁺  = {dMasMax:F6}");
        sb.AppendLine($"  D⁻  = {dMenosMax:F6}");
        sb.AppendLine($"  D   = {d:F6}");
        sb.AppendLine($"  D crítico = {critico:F6}");
        sb.AppendLine();
        sb.AppendLine("Decisión:");
        sb.AppendLine($"  ¿D ≤ D crítico?   {d:F6} ≤ {critico:F6}   →   {(aceptaH0 ? "CUMPLE" : "NO CUMPLE")}");
        sb.AppendLine();
        sb.AppendLine($"  >>> {Estadistica.Veredicto(aceptaH0)} <<<");

        return new ResultadoPruebaValidacion { Reporte = sb.ToString(), SeAceptaH0 = aceptaH0 };
    }
}
