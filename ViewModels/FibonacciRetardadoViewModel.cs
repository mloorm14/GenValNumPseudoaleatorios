using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GenValNumAl.Models;
using GenValNumAl.Services;

namespace GenValNumAl.ViewModels;

public partial class FibonacciRetardadoViewModel : GeneradorViewModelBase
{
    public override string Titulo => "Fibonacci Retardado";

    /// <summary>Semillas separadas por comas — exactamente k valores.</summary>
    // Válido: 0<j<k (3<7), 7 semillas para k=7, no todas cero, m=10 par pero hay impares entre las semillas
    [ObservableProperty] private string _semillas = "1,2,3,4,5,6,7";
    [ObservableProperty] private string _j        = "3";
    [ObservableProperty] private string _k        = "7";
    [ObservableProperty] private string _m        = "10";
    [ObservableProperty] private string _n        = "12";

    private readonly ServicioFibonacciRetardado _servicio = new();

    protected override ResultadoGeneracion EjecutarGeneracion()
    {
        if (!int.TryParse(J.Trim(), out int j))
            throw new ExcepcionValidacion("Error: 'j' debe ser un número entero positivo.");
        if (!int.TryParse(K.Trim(), out int k))
            throw new ExcepcionValidacion("Error: 'k' debe ser un número entero positivo.");
        if (!long.TryParse(M.Trim(), out long m))
            throw new ExcepcionValidacion("Error: 'm' debe ser un número entero positivo.");
        if (!int.TryParse(N.Trim(), out int n))
            throw new ExcepcionValidacion("Error: 'n' debe ser un número entero positivo.");

        // Interpretar las semillas separadas por comas
        var partes = Semillas.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0)
            throw new ExcepcionValidacion(
                "Error: Ingrese las semillas iniciales separadas por comas (ej: 1, 6, 3, 2, 9).");

        var semillasAnalizadas = new long[partes.Length];
        for (int i = 0; i < partes.Length; i++)
        {
            if (!long.TryParse(partes[i].Trim(), out long valor))
                throw new ExcepcionValidacion(
                    $"Error: El valor '{partes[i].Trim()}' no es un número entero válido. " +
                    "Todas las semillas deben ser enteros separados por comas.");
            semillasAnalizadas[i] = valor;
        }

        return _servicio.Generar(semillasAnalizadas, j, k, m, n);
    }
}
