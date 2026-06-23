namespace GenValNumAl.Models;

/// <summary>Resultado de ejecutar una prueba de validación: reporte formateado y veredicto sobre H0.</summary>
public sealed class ResultadoPruebaValidacion
{
    public required string Reporte { get; init; }
    public required bool SeAceptaH0 { get; init; }
}
