using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GenValNumAl.Models;

namespace GenValNumAl.Services.Validation;

/// <summary>Prueba de Poker: clasifica cada número (5 dígitos) según el patrón de repetición de sus dígitos.</summary>
public sealed class PokerTest : IValidationTest
{
    public string Nombre => "Prueba de Poker";

    private static readonly string[] Categorias =
    {
        "Todos diferentes", "Un par", "Dos pares", "Tercia", "Full house", "Poker", "Quintilla"
    };

    private static readonly double[] Probabilidades =
    {
        0.3024, 0.5040, 0.1080, 0.0720, 0.0090, 0.0045, 0.0001
    };

    public ValidationTestResult Ejecutar(List<double> datos, ValidationParameters parametros)
    {
        int n = datos.Count;
        if (n < 1)
            throw new ValidationException("Se necesita al menos 1 dato para la prueba de Poker.");

        double alpha = parametros.Alpha;
        var observados = new int[Categorias.Length];

        foreach (var valor in datos)
            observados[ClasificarCategoria(valor)]++;

        var sb = new StringBuilder();
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine("  PRUEBA DE POKER");
        sb.AppendLine("════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("Hipótesis:");
        sb.AppendLine("  H0: los números de la muestra son estadísticamente independientes");
        sb.AppendLine("  H1: los números de la muestra NO son independientes");
        sb.AppendLine();
        sb.AppendLine("Fórmulas:");
        sb.AppendLine("  Cada rᵢ se multiplica por 100,000 para obtener 5 dígitos y se clasifica por su patrón.");
        sb.AppendLine("  Eᵢ = n · pᵢ        χ²₀ = Σ (Oᵢ - Eᵢ)² / Eᵢ        Crítico: χ²(1-α, 6)  [Wilson-Hilferty]");
        sb.AppendLine();
        sb.AppendLine("Datos de entrada:");
        sb.AppendLine($"  n (tamaño de muestra) = {n}");
        sb.AppendLine($"  α (alpha)             = {alpha.ToString("0.####", CultureInfo.InvariantCulture)}");
        sb.AppendLine();
        sb.AppendLine("Tabla de frecuencias:");
        sb.AppendLine("  Categoría             pᵢ        Oᵢ        Eᵢ        (Oᵢ-Eᵢ)²/Eᵢ");

        double estadistico = 0;
        for (int i = 0; i < Categorias.Length; i++)
        {
            double esperado = n * Probabilidades[i];

            // Defensa adicional: las probabilidades son constantes fijas > 0, así que Eᵢ no debería
            // llegar a 0 salvo n = 0 (ya descartado arriba). Se deja explícito para evitar NaN/Infinity.
            if (esperado <= 0)
                throw new ValidationException("No se puede calcular la frecuencia esperada para la prueba de Poker con esta muestra.");

            double aporte = Math.Pow(observados[i] - esperado, 2) / esperado;
            estadistico += aporte;
            sb.AppendLine($"  {Categorias[i],-20}  {Probabilidades[i],6:F4}   {observados[i],6}   {esperado,8:F2}   {aporte,10:F4}");
        }

        const int gradosLibertad = 6;
        double critico = Statistics.ChiSquareCriticalValue(gradosLibertad, 1 - alpha);
        bool aceptaH0 = estadistico <= critico;

        sb.AppendLine();
        sb.AppendLine("Resultados:");
        sb.AppendLine($"  χ²₀ (estadístico calculado) = {estadistico:F6}");
        sb.AppendLine($"  χ²(1-α, 6) (crítico)        = {critico:F6}");
        sb.AppendLine();
        sb.AppendLine("Decisión:");
        sb.AppendLine($"  ¿χ²₀ ≤ χ²(1-α, 6)?   {estadistico:F6} ≤ {critico:F6}   →   {(aceptaH0 ? "CUMPLE" : "NO CUMPLE")}");
        sb.AppendLine();
        sb.AppendLine($"  >>> {Statistics.Veredicto(aceptaH0)} <<<");

        return new ValidationTestResult { Reporte = sb.ToString(), SeAceptaH0 = aceptaH0 };
    }

    private static int ClasificarCategoria(double valor)
    {
        long cincoDigitos = (long)Math.Floor(valor * 100000.0);
        if (cincoDigitos >= 100000) cincoDigitos = 99999;
        if (cincoDigitos < 0) cincoDigitos = 0;

        string texto = cincoDigitos.ToString("D5", CultureInfo.InvariantCulture);
        var conteos = texto.GroupBy(c => c).Select(g => g.Count()).OrderByDescending(c => c).ToList();

        return conteos switch
        {
            [5] => 6,             // Quintilla
            [4, 1] => 5,          // Poker
            [3, 2] => 4,          // Full house
            [3, 1, 1] => 3,       // Tercia
            [2, 2, 1] => 2,       // Dos pares
            [2, 1, 1, 1] => 1,    // Un par
            _ => 0                // Todos diferentes
        };
    }
}
