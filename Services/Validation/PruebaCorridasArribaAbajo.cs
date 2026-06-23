using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GenValNumAl.Models;

namespace GenValNumAl.Services.Validation;

/// <summary>Corridas Arriba y Abajo: analiza la alternancia de subidas/bajadas entre valores consecutivos.</summary>
public sealed class PruebaCorridasArribaAbajo : IPruebaValidacion
{
    public string Nombre => "Corridas Arriba y Abajo";

    public ResultadoPruebaValidacion Ejecutar(List<double> datos, ParametrosValidacion parametros)
    {
        int n = datos.Count;
        if (n < 2)
            throw new ExcepcionValidacion("Se necesitan al menos 2 datos para la prueba de corridas arriba y abajo.");

        double alpha = parametros.Alpha;

        var signos = new List<int>(n - 1);
        for (int i = 1; i < n; i++)
            signos.Add(datos[i] > datos[i - 1] ? 1 : 0);

        int corridas = Estadistica.ContarCorridas(signos);

        double mu = (2.0 * n - 1) / 3.0;
        double varianza = (16.0 * n - 29) / 90.0;
        double z0 = Math.Abs(corridas - mu) / Math.Sqrt(varianza);

        double zCritico = Estadistica.InversaNormalEstandar(1 - alpha / 2.0);
        bool aceptaH0 = z0 <= zCritico;

        var sb = new StringBuilder();
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine("  CORRIDAS ARRIBA Y ABAJO");
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("Hipótesis:");
        sb.AppendLine("  H0: la secuencia de subidas y bajadas es independiente (aleatoria)");
        sb.AppendLine("  H1: la secuencia de subidas y bajadas NO es independiente");
        sb.AppendLine();
        sb.AppendLine("Fórmulas:");
        sb.AppendLine("  μ = (2n - 1) / 3              σ² = (16n - 29) / 90");
        sb.AppendLine("  Z₀ = |C₀ - μ| / σ              Crítico: Z(1-α/2)");
        sb.AppendLine();
        sb.AppendLine("Datos de entrada:");
        sb.AppendLine($"  n (tamaño de muestra) = {n}");
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
        sb.AppendLine($"  ¿Z₀ ≤ Z crítico?   {z0:F6} ≤ {zCritico:F4}   →   {(aceptaH0 ? "CUMPLE" : "NO CUMPLE")}");
        sb.AppendLine();
        sb.AppendLine($"  >>> {Estadistica.Veredicto(aceptaH0)} <<<");

        return new ResultadoPruebaValidacion { Reporte = sb.ToString(), SeAceptaH0 = aceptaH0 };
    }
}
