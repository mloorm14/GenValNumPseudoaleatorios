using System.Collections.Generic;
using GenValNumAl.Models;

namespace GenValNumAl.Services.Validation;

public interface IValidationTest
{
    string Nombre { get; }

    /// <summary>Ejecuta la prueba sobre la muestra y devuelve el reporte y el veredicto. Puede lanzar <see cref="ValidationException"/>.</summary>
    ValidationTestResult Ejecutar(List<double> datos, ValidationParameters parametros);
}
