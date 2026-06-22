using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using GenValNumAl.Models;
using GenValNumAl.Services;

namespace GenValNumAl.ViewModels;

public partial class LaggedFibonacciViewModel : GeneratorViewModelBase
{
    public override string Title => "Fibonacci Retardado";

    /// <summary>Semillas separadas por comas — exactamente k valores.</summary>
    // Válido: 0<j<k (3<7), 7 semillas para k=7, no todas cero, m=10 par pero hay impares en seeds
    [ObservableProperty] private string _seeds = "1,2,3,4,5,6,7";
    [ObservableProperty] private string _j     = "3";
    [ObservableProperty] private string _k     = "7";
    [ObservableProperty] private string _m     = "10";
    [ObservableProperty] private string _n     = "12";

    private readonly LaggedFibonacciService _service = new();

    protected override IEnumerable<GeneratedNumber> ExecuteGenerate()
    {
        if (!int.TryParse(J.Trim(), out int j))
            throw new ValidationException("Error: 'j' debe ser un número entero positivo.");
        if (!int.TryParse(K.Trim(), out int k))
            throw new ValidationException("Error: 'k' debe ser un número entero positivo.");
        if (!long.TryParse(M.Trim(), out long m))
            throw new ValidationException("Error: 'm' debe ser un número entero positivo.");
        if (!int.TryParse(N.Trim(), out int n))
            throw new ValidationException("Error: 'n' debe ser un número entero positivo.");

        // Parsear semillas separadas por comas
        var parts = Seeds.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            throw new ValidationException(
                "Error: Ingrese las semillas iniciales separadas por comas (ej: 1, 6, 3, 2, 9).");

        var parsedSeeds = new long[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!long.TryParse(parts[i].Trim(), out long val))
                throw new ValidationException(
                    $"Error: El valor '{parts[i].Trim()}' no es un número entero válido. " +
                    "Todas las semillas deben ser enteros separados por comas.");
            parsedSeeds[i] = val;
        }

        return _service.Generate(parsedSeeds, j, k, m, n);
    }
}
