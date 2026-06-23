using System.Collections.Generic;

namespace GenValNumAl.Models;

/// <summary>Resultado de generar una secuencia: las filas obtenidas y, si corresponde, el ciclo detectado.</summary>
public sealed class ResultadoGeneracion
{
    public required List<NumeroGenerado> Numeros { get; init; }
    public InfoCiclo? Ciclo { get; init; }
}
