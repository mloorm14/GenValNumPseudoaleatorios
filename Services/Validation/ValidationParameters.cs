namespace GenValNumAl.Services.Validation;

public sealed class ValidationParameters
{
    public double Alpha { get; init; } = 0.05;

    /// <summary>Número de intervalos (k), usado únicamente por la prueba Chi-Cuadrado de uniformidad.</summary>
    public int Intervalos { get; init; } = 10;
}
