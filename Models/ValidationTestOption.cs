using GenValNumAl.Services.Validation;

namespace GenValNumAl.Models;

/// <summary>Entrada del ComboBox de pruebas: nombre visible + instancia del servicio que la ejecuta.</summary>
public sealed class ValidationTestOption
{
    public required string Nombre { get; init; }
    public required IValidationTest Prueba { get; init; }
    public bool RequiereIntervalos { get; init; }
}
