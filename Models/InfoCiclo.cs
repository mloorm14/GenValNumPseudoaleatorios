namespace GenValNumAl.Models;

/// <summary>Información sobre un ciclo o degeneración detectada al recorrer la secuencia generada.</summary>
public sealed class InfoCiclo
{
    public required int IteracionInicio { get; init; }
    public required int IteracionDeteccion { get; init; }
    public required int LongitudPeriodo { get; init; }
    public required string Mensaje { get; init; }
}
