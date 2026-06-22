using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using GenValNumAl.Models;
using GenValNumAl.Services;

namespace GenValNumAl.ViewModels;

public partial class MidSquareViewModel : GeneratorViewModelBase
{
    public override string Title => "Mitad del Cuadrado";

    [ObservableProperty] private string _x0     = "1234";
    [ObservableProperty] private string _digits = "4";
    [ObservableProperty] private string _n      = "10";

    private readonly MidSquareService _service = new();

    protected override IEnumerable<GeneratedNumber> ExecuteGenerate()
    {
        if (!long.TryParse(X0.Trim(), out long x0))
            throw new ValidationException("Error: 'X₀' debe ser un número entero positivo.");
        if (!int.TryParse(Digits.Trim(), out int digits))
            throw new ValidationException("Error: 'Dígitos' debe ser un número entero positivo par.");
        if (!int.TryParse(N.Trim(), out int n))
            throw new ValidationException("Error: 'n' debe ser un número entero positivo.");

        return _service.Generate(x0, digits, n);
    }
}
