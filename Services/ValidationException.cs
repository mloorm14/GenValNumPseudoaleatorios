using System;

namespace GenValNumAl.Services;

/// <summary>
/// Se lanza cuando los parámetros del generador no cumplen las condiciones matemáticas requeridas.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
