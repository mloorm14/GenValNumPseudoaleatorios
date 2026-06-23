using System.Collections.Generic;
using GenValNumAl.Models;

namespace GenValNumAl.Services.Validation;

public interface IPruebaValidacion
{
    string Nombre { get; }

    /// <summary>Ejecutar la prueba sobre la muestra y devolver el reporte y el veredicto. Puede lanzar <see cref="ExcepcionValidacion"/>.</summary>
    ResultadoPruebaValidacion Ejecutar(List<double> datos, ParametrosValidacion parametros);
}
