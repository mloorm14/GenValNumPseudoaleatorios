using System;

namespace GenValNumAl.Services;

/// <summary>
/// Excepción lanzada cuando los parámetros del generador o de una prueba de validación
/// no cumplen las condiciones matemáticas requeridas.
/// </summary>
public sealed class ExcepcionValidacion : Exception
{
    public ExcepcionValidacion(string message) : base(message) { }
}
