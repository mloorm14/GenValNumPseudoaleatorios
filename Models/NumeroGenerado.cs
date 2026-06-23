namespace GenValNumAl.Models;

/// <summary>Una fila de la secuencia generada: índice, estado interno y número pseudoaleatorio en [0,1).</summary>
public sealed class NumeroGenerado
{
    public int    N  { get; init; }
    public long   Xn { get; init; }
    public double Un { get; init; }
}
