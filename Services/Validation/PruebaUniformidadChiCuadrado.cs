using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GenValNumAl.Models;

namespace GenValNumAl.Services.Validation;

/// <summary>Prueba de Uniformidad Chi-Cuadrado: divide [0,1) en k intervalos y compara frecuencias observadas vs. esperadas.</summary>
public sealed class PruebaUniformidadChiCuadrado : IPruebaValidacion
{
    public string Nombre => "Prueba de Uniformidad Chi-Cuadrado";

    public ResultadoPruebaValidacion Ejecutar(List<double> datos, ParametrosValidacion parametros)
    {
        int n = datos.Count;
        int k = parametros.Intervalos;
        if (k < 2)
            throw new ExcepcionValidacion("El número de intervalos (k) debe ser mayor o igual a 2.");
        if (n < k)
            throw new ExcepcionValidacion("La muestra debe tener al menos tantos datos como intervalos (k).");

        double alpha = parametros.Alpha;
        double ancho = 1.0 / k;
        var observados = new int[k];

        foreach (var valor in datos)
        {
            int indice = (int)(valor / ancho);
            if (indice < 0) indice = 0;
            if (indice >= k) indice = k - 1; // Cubrir el caso límite valor == 1.0
            observados[indice]++;
        }

        double esperado = (double)n / k;

        // Con la verificación "n < k" de arriba, Eᵢ = n/k siempre es >= 1; se deja la guarda
        // explícita para evitar una división por cero o un NaN si esa invariante cambiara.
        if (esperado <= 0)
            throw new ExcepcionValidacion("No se puede calcular la frecuencia esperada (Eᵢ = n/k resultó en 0); revise n y k.");

        double estadistico = 0;
        var sb = new StringBuilder();

        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine("  PRUEBA DE UNIFORMIDAD CHI-CUADRADO");
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("Hipótesis:");
        sb.AppendLine("  H0: los números provienen de una distribución uniforme(0,1)");
        sb.AppendLine("  H1: los números NO provienen de una distribución uniforme(0,1)");
        sb.AppendLine();
        sb.AppendLine("Fórmulas:");
        sb.AppendLine("  Eᵢ = n / k");
        sb.AppendLine("  χ²₀ = Σ (Oᵢ - Eᵢ)² / Eᵢ        Crítico: χ²(1-α, k-1)  [aprox. Wilson-Hilferty]");
        sb.AppendLine();
        sb.AppendLine("Datos de entrada:");
        sb.AppendLine($"  n (tamaño de muestra) = {n}");
        sb.AppendLine($"  k (intervalos)        = {k}");
        sb.AppendLine($"  α (alpha)             = {alpha.ToString("0.####", CultureInfo.InvariantCulture)}");
        sb.AppendLine();
        sb.AppendLine("Tabla de frecuencias:");
        sb.AppendLine("  Intervalo                Oᵢ        Eᵢ        (Oᵢ-Eᵢ)²/Eᵢ");

        for (int i = 0; i < k; i++)
        {
            double aporte = Math.Pow(observados[i] - esperado, 2) / esperado;
            estadistico += aporte;
            string intervalo = $"[{i * ancho:F4}, {(i + 1) * ancho:F4})".PadRight(22);
            sb.AppendLine($"  {intervalo} {observados[i],6}   {esperado,8:F2}   {aporte,10:F4}");
        }

        int gradosLibertad = k - 1;
        double critico = Estadistica.ValorCriticoChiCuadrado(gradosLibertad, 1 - alpha);
        bool aceptaH0 = estadistico <= critico;

        sb.AppendLine();
        sb.AppendLine("Resultados:");
        sb.AppendLine($"  χ²₀ (estadístico calculado) = {estadistico:F6}");
        sb.AppendLine($"  χ²(1-α, k-1) (crítico)      = {critico:F6}");
        sb.AppendLine();
        sb.AppendLine("Decisión:");
        sb.AppendLine($"  ¿χ²₀ ≤ χ²(1-α, k-1)?   {estadistico:F6} ≤ {critico:F6}   →   {(aceptaH0 ? "CUMPLE" : "NO CUMPLE")}");
        sb.AppendLine();
        sb.AppendLine($"  >>> {Estadistica.Veredicto(aceptaH0)} <<<");

        return new ResultadoPruebaValidacion { Reporte = sb.ToString(), SeAceptaH0 = aceptaH0 };
    }
}
