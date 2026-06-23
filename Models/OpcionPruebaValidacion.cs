using GenValNumAl.Services.Validation;

namespace GenValNumAl.Models;

/// <summary>Entrada del ComboBox de pruebas: nombre visible + instancia de la prueba que la ejecuta.</summary>
public sealed class OpcionPruebaValidacion
{
    public required string Nombre { get; init; }
    public required IPruebaValidacion Prueba { get; init; }
    public bool RequiereIntervalos { get; init; }
}
