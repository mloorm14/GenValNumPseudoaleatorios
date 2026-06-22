using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using GenValNumAl.Models;
using GenValNumAl.Services;

namespace GenValNumAl.ViewModels;

public partial class MultiplicativeCongruentialViewModel : GeneratorViewModelBase
{
    public override string Title => "Congruencial Multiplicativo";

    // Válido: X0≠0, gcd(3,16)=1 (3 impar, 16 potencia de 2)
    [ObservableProperty] private string _x0 = "3";
    [ObservableProperty] private string _a  = "5";
    [ObservableProperty] private string _m  = "16";
    [ObservableProperty] private string _n  = "4";

    private readonly MultiplicativeCongruentialService _service = new();

    protected override IEnumerable<GeneratedNumber> ExecuteGenerate()
    {
        if (!long.TryParse(X0.Trim(), out long x0))
            throw new ValidationException("Error: 'X₀' debe ser un número entero positivo.");
        if (!long.TryParse(A.Trim(), out long a))
            throw new ValidationException("Error: 'a' debe ser un número entero positivo.");
        if (!long.TryParse(M.Trim(), out long m))
            throw new ValidationException("Error: 'm' debe ser un número entero positivo.");
        if (!int.TryParse(N.Trim(), out int n))
            throw new ValidationException("Error: 'n' debe ser un número entero positivo.");

        return _service.Generate(x0, a, m, n);
    }
}
