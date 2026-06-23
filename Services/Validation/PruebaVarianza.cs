using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GenValNumAl.Models;

namespace GenValNumAl.Services.Validation;

/// <summary>Prueba de Varianza: verifica que la varianza muestral sea estadísticamente igual a 1/12.</summary>
public sealed class PruebaVarianza : IPruebaValidacion
{
    public string Nombre => "Prueba de Varianza";

    public ResultadoPruebaValidacion Ejecutar(List<double> datos, ParametrosValidacion parametros)
    {
        int n = datos.Count;
        if (n < 2)
            throw new ExcepcionValidacion("Se necesitan al menos 2 datos para la prueba de varianza.");

        double alpha = parametros.Alpha;
        double media = datos.Average();
        double sumaCuadrados = datos.Sum(x => (x - media) * (x - media));
        double varianzaMuestral = sumaCuadrados / (n - 1);

        // Estadístico = (n-1)*S² / (1/12), distribuido Chi-Cuadrado con (n-1) grados de libertad bajo H0.
        double estadistico = (n - 1) * varianzaMuestral / (1.0 / 12.0);

        int gradosLibertad = n - 1;
        double chiInferior = Estadistica.ValorCriticoChiCuadrado(gradosLibertad, alpha / 2.0);
        double chiSuperior = Estadistica.ValorCriticoChiCuadrado(gradosLibertad, 1 - alpha / 2.0);

        bool aceptaH0 = estadistico >= chiInferior && estadistico <= chiSuperior;

        var sb = new StringBuilder();
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine("  PRUEBA DE VARIANZA");
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("Hipótesis:");
        sb.AppendLine("  H0: σ² = 1/12  (la varianza de los números generados es 1/12)");
        sb.AppendLine("  H1: σ² ≠ 1/12");
        sb.AppendLine();
        sb.AppendLine("Fórmulas:");
        sb.AppendLine("  S² = Σ(rᵢ - X̄)² / (n - 1)");
        sb.AppendLine("  χ²₀ = (n - 1)·S² / (1/12)");
        sb.AppendLine("  Crítico: χ²(α/2, n-1)  y  χ²(1-α/2, n-1)  [aprox. Wilson-Hilferty]");
        sb.AppendLine();
        sb.AppendLine("Datos de entrada:");
        sb.AppendLine($"  n (tamaño de muestra)       = {n}");
        sb.AppendLine($"  α (alpha)                   = {alpha.ToString("0.####", CultureInfo.InvariantCulture)}");
        sb.AppendLine($"  Grados de libertad (n-1)    = {gradosLibertad}");
        sb.AppendLine();
        sb.AppendLine("Resultados:");
        sb.AppendLine($"  S² (varianza muestral)      = {varianzaMuestral:F6}");
        sb.AppendLine($"  χ²₀ (estadístico calculado) = {estadistico:F6}");
        sb.AppendLine($"  χ²(α/2, n-1)   (crítico LI)  = {chiInferior:F6}");
        sb.AppendLine($"  χ²(1-α/2, n-1) (crítico LS)  = {chiSuperior:F6}");
        sb.AppendLine();
        sb.AppendLine("Decisión:");
        sb.AppendLine($"  ¿χ²(α/2) ≤ χ²₀ ≤ χ²(1-α/2)?   {chiInferior:F6} ≤ {estadistico:F6} ≤ {chiSuperior:F6}   →   {(aceptaH0 ? "CUMPLE" : "NO CUMPLE")}");
        sb.AppendLine();
        sb.AppendLine($"  >>> {Estadistica.Veredicto(aceptaH0)} <<<");

        return new ResultadoPruebaValidacion { Reporte = sb.ToString(), SeAceptaH0 = aceptaH0 };
    }
}
